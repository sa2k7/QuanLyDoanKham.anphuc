import * as signalR from '@microsoft/signalr'

const HUB_URL = (import.meta.env.VITE_API_BASE_URL || '') + '/hubs/attendance'

/**
 * AttendanceHubService — Singleton SignalR client for realtime attendance updates.
 *
 * Server events handled:
 *   - StaffCheckedIn(data)   → a staff member checked in to a team
 *   - StaffCheckedOut(data)  → a staff member checked out from a team
 */
class AttendanceHubService {
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

        // Listen for check-in events
        this.connection.on('StaffCheckedIn', (data) => {
            this._emit('StaffCheckedIn', data)
        })

        // Listen for check-out events
        this.connection.on('StaffCheckedOut', (data) => {
            this._emit('StaffCheckedOut', data)
        })

        // Listen for schedule update events (new assignment)
        this.connection.on('ScheduleUpdated', (data) => {
            this._emit('ScheduleUpdated', data)
        })

        this.connection.onreconnected(() => this._emit('Reconnected', null))
        this.connection.onclose((error) => this._emit('Disconnected', error ?? null))

        try {
            await this.connection.start()
        } catch (err) {
            console.error('[AttendanceHub] Connection failed:', err)
            setTimeout(() => this.start(), 5000)
        }
    }

    async joinTeam(teamId) {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('JoinTeamGroup', teamId)
        }
    }

    async leaveTeam(teamId) {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('LeaveTeamGroup', teamId)
        }
    }

    /** Register a listener for all hub events. Returns an unsubscribe function. */
    onUpdate(callback) {
        this.listeners.push(callback)
        return () => { this.listeners = this.listeners.filter(l => l !== callback) }
    }

    _emit(event, payload) {
        this.listeners.forEach(l => l(event, payload))
    }
}

const attendanceHub = new AttendanceHubService()
export default attendanceHub
