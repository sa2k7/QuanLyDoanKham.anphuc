/**
 * useFinancialReport.js
 *
 * Composable that owns all data-fetching for the Financial Dashboard.
 * Calls the new settlement-based endpoints:
 *   GET /api/reports/financial/summary   → period aggregate + per-contract rows
 *   GET /api/reports/financial/trend     → monthly/quarterly series
 *   GET /api/dashboard/overview          → KPI cards + alerts
 *
 * Falls back gracefully when the user lacks BaoCao.ViewFinance permission
 * (the API returns 403; we surface an empty state, not an error crash).
 */
import { ref, computed } from 'vue'
import apiClient from '../services/apiClient'

// ── Default shapes ────────────────────────────────────────────────────────────
const emptyOverview = () => ({
  financial: {
    totalRevenue: 0,
    totalCost: 0,
    grossProfit: 0,
    profitMargin: 0,
    dataSource: 'CostSnapshot',
  },
  operations: {
    activeGroups: 0,
    completedGroups: 0,
    pendingContracts: 0,
    totalStaffDeployed: 0,
  },
  alerts: [],
})

const emptySummary = () => ({
  summary: {
    totalRevenue: 0,
    packageRevenue: 0,
    extraRevenue: 0,
    laborCost: 0,
    supplyCost: 0,
    overheadCost: 0,
    totalCost: 0,
    grossProfit: 0,
    profitMargin: 0,
    contractCount: 0,
    lowMarginCount: 0,
  },
  byContract: [],
})

const emptyTrend = () => ({ series: [] })

// ── Composable ────────────────────────────────────────────────────────────────
export function useFinancialReport() {
  const loading      = ref(false)
  const loadingTrend = ref(false)
  const error        = ref(null)

  const overview = ref(emptyOverview())
  const summary  = ref(emptySummary())
  const trend    = ref(emptyTrend())

  // ── Fetch overview (dashboard KPIs + alerts) ──────────────────────────────
  async function fetchOverview(from, to) {
    try {
      const res = await apiClient.get('/api/dashboard/overview', {
        params: { from, to },
      })
      overview.value = res.data ?? emptyOverview()
    } catch (err) {
      if (err.response?.status !== 403) {
        console.error('[useFinancialReport] overview error', err)
      }
      overview.value = emptyOverview()
    }
  }

  // ── Fetch financial summary (aggregate + per-contract table) ─────────────
  async function fetchSummary(from, to, companyId = null) {
    try {
      const params = { from, to }
      if (companyId) params.companyId = companyId
      const res = await apiClient.get('/api/reports/financial/summary', { params })
      summary.value = res.data ?? emptySummary()
    } catch (err) {
      if (err.response?.status !== 403) {
        console.error('[useFinancialReport] summary error', err)
      }
      summary.value = emptySummary()
    }
  }

  // ── Fetch trend series ────────────────────────────────────────────────────
  async function fetchTrend(from, to, granularity = 'month') {
    loadingTrend.value = true
    try {
      const res = await apiClient.get('/api/reports/financial/trend', {
        params: { from, to, granularity },
      })
      trend.value = res.data ?? emptyTrend()
    } catch (err) {
      if (err.response?.status !== 403) {
        console.error('[useFinancialReport] trend error', err)
      }
      trend.value = emptyTrend()
    } finally {
      loadingTrend.value = false
    }
  }

  // ── Load all at once ──────────────────────────────────────────────────────
  async function loadAll(from, to, granularity = 'month', companyId = null) {
    loading.value = true
    error.value   = null
    try {
      await Promise.all([
        fetchOverview(from, to),
        fetchSummary(from, to, companyId),
        fetchTrend(from, to, granularity),
      ])
    } catch (err) {
      error.value = err
    } finally {
      loading.value = false
    }
  }

  // ── Computed helpers ──────────────────────────────────────────────────────

  /** Margin status → colour token used by the UI */
  const marginStatusColor = computed(() => {
    const m = overview.value.financial.profitMargin
    if (m < 0)  return 'rose'
    if (m < 20) return 'amber'
    if (m < 35) return 'sky'
    return 'emerald'
  })

  /** Alerts split by type for the alert panel */
  const lossAlerts      = computed(() => overview.value.alerts.filter(a => a.type === 'LOSS'))
  const lowMarginAlerts = computed(() => overview.value.alerts.filter(a => a.type === 'LOW_MARGIN'))

  /** Top 5 contracts by gross profit for the leaderboard */
  const topContracts = computed(() =>
    [...(summary.value.byContract ?? [])]
      .sort((a, b) => b.grossProfit - a.grossProfit)
      .slice(0, 5)
  )

  /** Bottom 5 contracts by profit margin for the risk table */
  const worstContracts = computed(() =>
    [...(summary.value.byContract ?? [])]
      .sort((a, b) => a.profitMargin - b.profitMargin)
      .slice(0, 5)
  )

  return {
    loading,
    loadingTrend,
    error,
    overview,
    summary,
    trend,
    marginStatusColor,
    lossAlerts,
    lowMarginAlerts,
    topContracts,
    worstContracts,
    loadAll,
    fetchOverview,
    fetchSummary,
    fetchTrend,
  }
}
