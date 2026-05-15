import api from './api'

export default {
  getAll(filters = {}) {
    return api.get('/cash-movements', { params: filters })
  },
  createManual(data) {
    return api.post('/cash-movements/manual', data)
  },
  createTransfer(data) {
    return api.post('/cash-movements/transfer', data)
  }
}
