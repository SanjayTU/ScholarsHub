import api from './axiosInstance';

export function searchListings(params) {
  const query = {};
  Object.entries(params || {}).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') query[key] = value;
  });
  return api.get('/listings', { params: query }).then((r) => r.data);
}

export function getListing(id) {
  return api.get(`/listings/${id}`).then((r) => r.data);
}

export function getListingContact(id) {
  return api.get(`/listings/${id}/contact`).then((r) => r.data);
}
