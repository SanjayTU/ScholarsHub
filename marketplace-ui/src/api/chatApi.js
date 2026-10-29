import api from './axiosInstance';

export function startConversation(listingId) {
  return api.post('/conversations', { listingId }).then((r) => r.data);
}

export function getConversations() {
  return api.get('/conversations').then((r) => r.data);
}

export function getMessages(conversationId, after) {
  const params = after != null ? { after } : undefined;
  return api.get(`/conversations/${conversationId}/messages`, { params }).then((r) => r.data);
}

export function sendMessage(conversationId, body) {
  return api.post(`/conversations/${conversationId}/messages`, { body }).then((r) => r.data);
}

export function getUnreadCount() {
  return api.get('/conversations/unread-count').then((r) => r.data);
}
