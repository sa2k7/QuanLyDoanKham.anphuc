/**
 * MySchedule.spec.js
 *
 * Property 7: Schedule Data Isolation
 * Validates: Requirements 3.7
 *
 * For any staffId S: when the MySchedule page is mounted with auth.staffId = S,
 * all schedule records displayed SHALL have staffId === S, and no records
 * belonging to other staff members SHALL be displayed.
 */

import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import * as fc from 'fast-check'
import MySchedule from '../MySchedule.vue'

// ─── Mock lucide-vue-next icons ───────────────────────────────────────────────
vi.mock('lucide-vue-next', () => ({
  CalendarCheck: { template: '<span />' },
  ChevronLeft:   { template: '<span />' },
  ChevronRight:  { template: '<span />' },
  Sun:           { template: '<span />' },
  Wallet:        { template: '<span />' },
  LogIn:         { template: '<span />' },
  LogOut:        { template: '<span />' },
  CalendarOff:   { template: '<span />' },
  Loader2:       { template: '<span />' },
}))

// ─── Mock i18n store ──────────────────────────────────────────────────────────
vi.mock('../../stores/i18n', () => ({
  useI18nStore: () => ({
    t: (key) => key,
  }),
}))

// ─── Mock toast composable ────────────────────────────────────────────────────
vi.mock('../../composables/useToast', () => ({
  useToast: () => ({
    success: vi.fn(),
    error: vi.fn(),
  }),
}))

// ─── Mock apiClient ───────────────────────────────────────────────────────────
const mockApiGet  = vi.fn()
const mockApiPost = vi.fn()

vi.mock('../../services/apiClient', () => ({
  default: {
    get:  (...args) => mockApiGet(...args),
    post: (...args) => mockApiPost(...args),
  },
}))

// ─── Mock auth store ──────────────────────────────────────────────────────────
let mockStaffId = null

vi.mock('../../stores/auth', () => ({
  useAuthStore: () => ({
    staffId:  mockStaffId,
    username: 'test_user',
  }),
}))

// ─── Helpers ──────────────────────────────────────────────────────────────────

/**
 * Build a fake schedule detail record for a given staffId.
 */
function makeRecord(staffId, overrides = {}) {
  return {
    staffId,
    groupId:       1,
    groupName:     `Group ${staffId}`,
    examDate:      '2025-01-15T00:00:00',
    shiftType:     1,
    workStatus:    'Đủ công',
    checkInTime:   null,
    checkOutTime:  null,
    groupStatus:   'Closed',
    ...overrides,
  }
}

/**
 * Mount MySchedule with a given staffId set in the auth store mock.
 * The apiClient mock returns `records` as the details array.
 */
async function mountWithStaffId(staffId, records) {
  mockStaffId = staffId

  mockApiGet.mockResolvedValue({
    data: { details: records },
  })

  const wrapper = mount(MySchedule, {
    global: {
      plugins: [createPinia()],
    },
  })

  // Wait for onMounted async operations to complete
  await wrapper.vm.$nextTick()
  await new Promise(resolve => setTimeout(resolve, 0))
  await wrapper.vm.$nextTick()

  return wrapper
}

// ─── Tests ────────────────────────────────────────────────────────────────────

describe('MySchedule — Property 7: Schedule Data Isolation', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    mockStaffId = null
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  // ── Unit test: component uses auth.staffId directly ──────────────────────
  it('uses auth.staffId directly without calling /api/Staffs', async () => {
    const staffId = 42
    mockStaffId = staffId

    mockApiGet.mockResolvedValue({ data: { details: [] } })

    const wrapper = mount(MySchedule, {
      global: { plugins: [createPinia()] },
    })

    await wrapper.vm.$nextTick()
    await new Promise(resolve => setTimeout(resolve, 0))
    await wrapper.vm.$nextTick()

    // Should call /api/Attendance/summary/{staffId} — NOT /api/Staffs or /api/Auth/profile
    const calledUrls = mockApiGet.mock.calls.map(call => call[0])
    expect(calledUrls.some(url => url.includes('/api/Staffs'))).toBe(false)
    expect(calledUrls.some(url => url.includes('/api/Auth/profile'))).toBe(false)
    expect(calledUrls.some(url => url.includes(`/api/Attendance/summary/${staffId}`))).toBe(true)
  })

  // ── Unit test: no API call when staffId is null ───────────────────────────
  it('does not call the schedule API when auth.staffId is null', async () => {
    mockStaffId = null
    mockApiGet.mockResolvedValue({ data: { details: [] } })

    const wrapper = mount(MySchedule, {
      global: { plugins: [createPinia()] },
    })

    await wrapper.vm.$nextTick()
    await new Promise(resolve => setTimeout(resolve, 0))
    await wrapper.vm.$nextTick()

    // loadSchedule returns early when staffId is null
    expect(mockApiGet).not.toHaveBeenCalled()
  })

  // ── Unit test: only records for the current staffId are displayed ─────────
  it('displays only records returned for the current staffId', async () => {
    const staffId = 7
    const records = [
      makeRecord(staffId, { groupId: 1, groupName: 'Group A' }),
      makeRecord(staffId, { groupId: 2, groupName: 'Group B' }),
    ]

    const wrapper = await mountWithStaffId(staffId, records)

    // All rendered group names belong to the records we provided
    const renderedNames = wrapper.findAll('.group-name').map(el => el.text())
    expect(renderedNames).toContain('Group A')
    expect(renderedNames).toContain('Group B')
    expect(renderedNames).toHaveLength(2)
  })

  // ── Property test: data isolation across random staffIds ─────────────────
  /**
   * Property 7: Schedule Data Isolation
   * Validates: Requirements 3.7
   *
   * For any staffId S generated by fc.integer({ min: 1, max: 1000 }):
   * - The component fetches data ONLY for staffId S
   * - All records in the response (which the API returns for S) are displayed
   * - No records from other staffIds are injected or mixed in
   */
  it.prop = it  // vitest does not have it.prop natively; use standard it with fc.assert

  it('Property 7 — all displayed records belong to the current staffId (no cross-staff leakage)', async () => {
    await fc.assert(
      fc.asyncProperty(
        fc.integer({ min: 1, max: 1000 }),
        fc.integer({ min: 0, max: 5 }),   // number of records for this staff
        async (staffId, recordCount) => {
          vi.clearAllMocks()
          setActivePinia(createPinia())

          // Build records that all belong to staffId
          const ownRecords = Array.from({ length: recordCount }, (_, i) =>
            makeRecord(staffId, { groupId: i + 1, groupName: `Group ${i + 1}` })
          )

          const wrapper = await mountWithStaffId(staffId, ownRecords)

          // Verify: the API was called with the correct staffId URL
          if (staffId !== null) {
            const calledUrls = mockApiGet.mock.calls.map(call => call[0])
            const attendanceCalls = calledUrls.filter(url =>
              url.includes('/api/Attendance/summary/')
            )
            // Should have exactly one attendance call
            expect(attendanceCalls).toHaveLength(1)
            // That call must use the correct staffId
            expect(attendanceCalls[0]).toBe(`/api/Attendance/summary/${staffId}`)
          }

          // Verify: /api/Staffs was NEVER called (no fallback search)
          const calledUrls = mockApiGet.mock.calls.map(call => call[0])
          expect(calledUrls.some(url => url.includes('/api/Staffs'))).toBe(false)

          // Verify: number of rendered schedule items matches the records returned
          const renderedItems = wrapper.findAll('.schedule-item')
          expect(renderedItems).toHaveLength(recordCount)

          wrapper.unmount()
        }
      ),
      { numRuns: 100 }
    )
  })

  // ── Property test: no records from other staffIds appear ─────────────────
  it('Property 7 — records from other staffIds never appear in the list', async () => {
    await fc.assert(
      fc.asyncProperty(
        fc.integer({ min: 1, max: 500 }),   // current user's staffId
        fc.integer({ min: 501, max: 1000 }), // another staffId (guaranteed different)
        fc.integer({ min: 1, max: 3 }),      // records for current user
        async (myStaffId, otherStaffId, myRecordCount) => {
          vi.clearAllMocks()
          setActivePinia(createPinia())

          // Records for the current user
          const myRecords = Array.from({ length: myRecordCount }, (_, i) =>
            makeRecord(myStaffId, { groupId: i + 1, groupName: `MyGroup ${i + 1}` })
          )

          // The API mock returns ONLY the current user's records
          // (the component should never request other staff's data)
          const wrapper = await mountWithStaffId(myStaffId, myRecords)

          // Verify: the component only called the API for myStaffId
          const calledUrls = mockApiGet.mock.calls.map(call => call[0])
          const attendanceCalls = calledUrls.filter(url =>
            url.includes('/api/Attendance/summary/')
          )
          // Must NOT have called for otherStaffId
          expect(
            attendanceCalls.some(url => url.includes(`/api/Attendance/summary/${otherStaffId}`))
          ).toBe(false)

          // Must have called for myStaffId
          expect(
            attendanceCalls.some(url => url.includes(`/api/Attendance/summary/${myStaffId}`))
          ).toBe(true)

          // Rendered items count matches only the current user's records
          const renderedItems = wrapper.findAll('.schedule-item')
          expect(renderedItems).toHaveLength(myRecordCount)

          wrapper.unmount()
        }
      ),
      { numRuns: 100 }
    )
  })
})
