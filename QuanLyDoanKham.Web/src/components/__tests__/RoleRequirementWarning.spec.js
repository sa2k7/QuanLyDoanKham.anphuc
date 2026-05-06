/**
 * RoleRequirementWarning.spec.js
 *
 * Unit tests for the RoleRequirementWarning component.
 *
 * Validates:
 *   - Under-assigned role renders a warning badge
 *   - Over-assigned role renders a warning badge
 *   - Exact match renders nothing
 *   - Empty requirements renders nothing
 *   - Mixed requirements render only the mismatched roles
 */

import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import RoleRequirementWarning from '../RoleRequirementWarning.vue'

// Mock lucide-vue-next icons used by the component
vi.mock('lucide-vue-next', () => ({
  AlertTriangle: { template: '<span class="icon-triangle" />' },
  AlertCircle:   { template: '<span class="icon-circle" />' },
}))

// ─── Helpers ──────────────────────────────────────────────────────────────────

function makeReq(role, requiredCount, assignedCount) {
  return { role, requiredCount, assignedCount }
}

function mountComponent(requirements) {
  return mount(RoleRequirementWarning, {
    props: { requirements }
  })
}

// ─── Tests ────────────────────────────────────────────────────────────────────

describe('RoleRequirementWarning', () => {

  it('renders nothing when requirements array is empty', () => {
    const wrapper = mountComponent([])
    expect(wrapper.findAll('[class*="rounded-lg"]')).toHaveLength(0)
  })

  it('renders nothing when requirements prop is omitted (default)', () => {
    const wrapper = mount(RoleRequirementWarning)
    expect(wrapper.findAll('[class*="rounded-lg"]')).toHaveLength(0)
  })

  it('renders nothing when all roles have exact counts', () => {
    const wrapper = mountComponent([
      makeReq('TiepNhan', 2, 2),
      makeReq('KhamNoi',  3, 3),
    ])
    expect(wrapper.findAll('[class*="rounded-lg"]')).toHaveLength(0)
  })

  it('renders an amber warning badge for an under-assigned role', () => {
    const wrapper = mountComponent([makeReq('TiepNhan', 3, 1)])
    const badges = wrapper.findAll('[class*="rounded-lg"]')
    expect(badges).toHaveLength(1)
    // amber styling
    expect(badges[0].classes().join(' ')).toContain('amber')
    // shows the role name
    expect(badges[0].text()).toContain('TiepNhan')
    // shows assigned/required counts
    expect(badges[0].text()).toContain('1')
    expect(badges[0].text()).toContain('3')
  })

  it('renders a rose warning badge for an over-assigned role', () => {
    const wrapper = mountComponent([makeReq('KhamNoi', 2, 5)])
    const badges = wrapper.findAll('[class*="rounded-lg"]')
    expect(badges).toHaveLength(1)
    // rose styling
    expect(badges[0].classes().join(' ')).toContain('rose')
    expect(badges[0].text()).toContain('KhamNoi')
    expect(badges[0].text()).toContain('5')
    expect(badges[0].text()).toContain('2')
  })

  it('renders only mismatched roles when mixed requirements are provided', () => {
    const wrapper = mountComponent([
      makeReq('TiepNhan', 2, 2),   // exact — no badge
      makeReq('KhamNoi',  3, 1),   // under — amber badge
      makeReq('LayMau',   1, 4),   // over  — rose badge
      makeReq('SieuAm',   2, 2),   // exact — no badge
    ])
    const badges = wrapper.findAll('[class*="rounded-lg"]')
    expect(badges).toHaveLength(2)

    const texts = badges.map(b => b.text())
    expect(texts.some(t => t.includes('KhamNoi'))).toBe(true)
    expect(texts.some(t => t.includes('LayMau'))).toBe(true)
    expect(texts.some(t => t.includes('TiepNhan'))).toBe(false)
    expect(texts.some(t => t.includes('SieuAm'))).toBe(false)
  })

  it('renders one badge per mismatched role (no duplicates)', () => {
    const requirements = [
      makeReq('KhamNgoai', 4, 2),
      makeReq('KhamSan',   1, 1),
    ]
    const wrapper = mountComponent(requirements)
    const badges = wrapper.findAll('[class*="rounded-lg"]')
    expect(badges).toHaveLength(1)
    expect(badges[0].text()).toContain('KhamNgoai')
  })

  it('under-assigned badge uses AlertTriangle icon', () => {
    const wrapper = mountComponent([makeReq('TiepNhan', 3, 1)])
    expect(wrapper.find('.icon-triangle').exists()).toBe(true)
    expect(wrapper.find('.icon-circle').exists()).toBe(false)
  })

  it('over-assigned badge uses AlertCircle icon', () => {
    const wrapper = mountComponent([makeReq('KhamNoi', 2, 5)])
    expect(wrapper.find('.icon-circle').exists()).toBe(true)
    expect(wrapper.find('.icon-triangle').exists()).toBe(false)
  })

  it('handles zero assigned count as under-assigned', () => {
    const wrapper = mountComponent([makeReq('LayMau', 2, 0)])
    const badges = wrapper.findAll('[class*="rounded-lg"]')
    expect(badges).toHaveLength(1)
    expect(badges[0].classes().join(' ')).toContain('amber')
  })
})
