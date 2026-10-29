import api from './axiosInstance';

export function reportListing(listingId, data) {
  return api.post(`/listings/${listingId}/report`, data).then((r) => r.data);
}
