import * as signalR from '@microsoft/signalr'

const HUB_URL = (import.meta.env.VITE_API_BASE_URL || '') + '/hub/queue'

/**
 * QueueHubService — Singleton SignalR client for the OMS Queue system.
 *
 * Server events handled:
 *   - QueueUpdated(type)              → legacy global refresh (backward compat)
 *   - StationQueueUpdated(stationCode)→ per-station refresh signal
 *   - NewPatientArrived(payload)      → Phase 2: rich patient-arrived data for Kiosk
 *   - PatientStatusChanged(payload)   → Phase 2: granular status update for Station view
 */
class QueueHubService {
    constructor() {
        this.connection = null
        /** @type {Array<(event: string, payload: any) => void>} */
        this.listeners = []
    }

    async start() {
        if (this.connection) return

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(HUB_URL, {
                accessTokenFactory: () => localStorage.getItem('auth_token'),
                skipNegotiation: false,
                transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Warning)
            .build()

        // ── Legacy events (kept for backward compat) ──────────────────────────
        this.connection.on('QueueUpdated', (type) => {
            this._emit('QueueUpdated', type)
        })

        this.connection.on('StationQueueUpdated', (stationCode) => {
            this._emit('StationQueueUpdated', stationCode)
        })

        // ── Phase 2: Rich payload events ──────────────────────────────────────
        /**
         * Fired when a new patient successfully checks in.
         * @param {{ medicalRecordId, fullName, queueNo, gender, firstStationCode, checkInAt }} payload
         */
        this.connection.on('NewPatientArrived', (payload) => {
            this._emit('NewPatientArrived', payload)
        })

        /**
         * Fired when a patient's station task status changes (start/complete).
         * @param {{ medicalRecordId, newStatus, stationCode, queueNo }} payload
         */
        this.connection.on('PatientStatusChanged', (payload) => {
            this._emit('PatientStatusChanged', payload)
        })

        this.connection.onreconnected(() => {
            console.log('[SignalR] Reconnected to QueueHub')
            this._emit('Reconnected', null)
        })
        this.connection.onreconnecting((error) => {
            console.warn('[SignalR] Reconnecting to QueueHub', error)
            this._emit('Reconnecting', error ?? null)
        })
        this.connection.onclose((error) => {
            console.warn('[SignalR] Disconnected from QueueHub', error)
            this._emit('Disconnected', error ?? null)
        })

        try {
            await this.connection.start()
            console.log('[SignalR] Connected to QueueHub')
        } catch (err) {
            console.error('[SignalR] Connection failed: ', err)
            // Retry after 5s — withAutomaticReconnect handles subsequent attempts
            setTimeout(() => this.start(), 5000)
        }
    }

    /** Register a listener for all hub events. */
    onUpdate(callback) {
        this.listeners.push(callback)
        // Return an unsubscribe function for clean teardown in components
        return () => {
            this.listeners = this.listeners.filter(l => l !== callback)
        }
    }

    async joinStation(stationCode) {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('JoinStationGroup', stationCode)
        }
    }

    async leaveStation(stationCode) {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('LeaveStationGroup', stationCode)
        }
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    _emit(event, payload) {
        this.listeners.forEach(l => l(event, payload))
    }
}

const queueHub = new QueueHubService()
export default queueHub
