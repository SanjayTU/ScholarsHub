import api from './axiosInstance';

const UPLOADS_BASE = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/api\/?$/, '') || 'https://localhost:7100';

const formConfig = {
  transformRequest: [
    (data, headers) => {
      delete headers['Content-Type'];
      if (headers.common) delete headers.common['Content-Type'];
      return data;
    },
  ],
};

function toFormData(data) {
  const fd = new FormData();
  fd.append('Title', data.title.trim());
  fd.append('CategoryId', String(data.categoryId));
  fd.append('EducationLevelId', String(data.educationLevelId));
  if (data.departmentId) fd.append('DepartmentId', String(data.departmentId));
  if (data.semesterOrClass) fd.append('SemesterOrClass', String(data.semesterOrClass));
  fd.append('Condition', String(data.condition));
  fd.append('Price', String(data.price));
  fd.append('Location', data.location.trim());
  fd.append('Description', data.description.trim());
  (data.images || []).forEach((file) => fd.append('Images', file));
  return fd;
}

export function listingImageUrl(path) {
  if (!path) return '';
  if (/^https?:/i.test(path)) return path;
  const origin = UPLOADS_BASE.startsWith('http') ? UPLOADS_BASE : 'https://localhost:7100';
  return `${origin}${path.startsWith('/') ? path : `/${path}`}`;
}

export function getMyListings() {
  return api.get('/listings/mine').then((r) => r.data);
}

export function createListing(data) {
  return api.post('/listings', toFormData(data), formConfig).then((r) => r.data);
}

export function updateListing(id, data) {
  return api.put(`/listings/${id}`, toFormData(data), formConfig).then((r) => r.data);
}

export function deleteListing(id) {
  return api.delete(`/listings/${id}`).then((r) => r.data);
}

export function changeListingStatus(id, status) {
  return api.patch(`/listings/${id}/status`, { status }).then((r) => r.data);
}
