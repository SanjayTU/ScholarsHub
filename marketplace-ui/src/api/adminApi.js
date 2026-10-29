import api from './axiosInstance';

export function getDashboard() {
  return api.get('/admin/dashboard').then((r) => r.data);
}

export function getUsers(params) {
  return api.get('/admin/users', { params }).then((r) => r.data);
}

export function toggleUserStatus(id) {
  return api.patch(`/admin/users/${id}/status`).then((r) => r.data);
}

export function deleteUser(id) {
  return api.delete(`/admin/users/${id}`).then((r) => r.data);
}

export function getAdminListings(params) {
  return api.get('/admin/listings', { params }).then((r) => r.data);
}

export function removeListing(id) {
  return api.patch(`/admin/listings/${id}/status`).then((r) => r.data);
}

export function getAdminCategories() {
  return api.get('/admin/categories').then((r) => r.data);
}

export function addCategory(data) {
  return api.post('/admin/categories', data).then((r) => r.data);
}

export function updateCategory(id, data) {
  return api.put(`/admin/categories/${id}`, data).then((r) => r.data);
}

export function toggleCategory(id) {
  return api.patch(`/admin/categories/${id}/status`).then((r) => r.data);
}

export function getReports(status) {
  const params = status ? { status } : undefined;
  return api.get('/admin/reports', { params }).then((r) => r.data);
}

export function handleReport(id, payload) {
  return api.patch(`/admin/reports/${id}`, payload).then((r) => r.data);
}
