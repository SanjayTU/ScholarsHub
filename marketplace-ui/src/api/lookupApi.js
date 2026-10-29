import api from './axiosInstance';

export function getEducationLevels() {
  return api.get('/lookups/education-levels').then((r) => r.data);
}

export function getDepartments() {
  return api.get('/lookups/departments').then((r) => r.data);
}

export function getCategories() {
  return api.get('/lookups/categories').then((r) => r.data);
}

export function getConditions() {
  return api.get('/lookups/conditions').then((r) => r.data);
}
