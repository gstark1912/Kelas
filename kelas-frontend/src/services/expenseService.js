import api from './api'

export default {
  getAll(filters = {}) {
    return api.get('/expenses', { params: filters })
  },
  getById(id) {
    return api.get(`/expenses/${id}`)
  },
  getByCategory(from, to) {
    return api.get('/expenses/by-category', { params: { from, to } })
  },
  create(data) {
    return api.post('/expenses', data)
  }
}
