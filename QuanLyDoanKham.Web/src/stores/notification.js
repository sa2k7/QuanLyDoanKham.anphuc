import { defineStore } from 'pinia';
import apiClient from '@/services/apiClient';

export const useNotificationStore = defineStore('notification', {
    state: () => ({
        notifications: [],
        unreadCount: 0,
        loading: false
    }),

    actions: {
        async fetchNotifications() {
            this.loading = true;
            try {
                const response = await apiClient.get('/api/notifications');
                this.notifications = response.data;
                await this.fetchUnreadCount();
            } catch (error) {
                console.error('Failed to fetch notifications:', error);
            } finally {
                this.loading = false;
            }
        },

        async fetchUnreadCount() {
            try {
                const response = await apiClient.get('/api/notifications/unread-count');
                this.unreadCount = response.data;
            } catch (error) {
                console.error('Failed to fetch unread count:', error);
            }
        },

        async markAsRead(id) {
            try {
                await apiClient.put(`/api/notifications/${id}/read`);
                const n = this.notifications.find(n => n.id === id);
                if (n && !n.isRead) {
                    n.isRead = true;
                    this.unreadCount = Math.max(0, this.unreadCount - 1);
                }
            } catch (error) {
                console.error('Failed to mark notification as read:', error);
            }
        },

        async markAllAsRead() {
            try {
                await apiClient.put('/api/notifications/read-all');
                this.notifications.forEach(n => n.isRead = true);
                this.unreadCount = 0;
            } catch (error) {
                console.error('Failed to mark all as read:', error);
            }
        },

        async deleteNotification(id) {
            try {
                await apiClient.delete(`/api/notifications/${id}`);
                this.notifications = this.notifications.filter(n => n.id !== id);
                await this.fetchUnreadCount();
            } catch (error) {
                console.error('Failed to delete notification:', error);
            }
        }
    }
});
