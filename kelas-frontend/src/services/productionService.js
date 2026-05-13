import api from './api'

export default {
    getAll(filters = {}) {
        return api.get('/production-batches', { params: filters })
    },
    getById(id) {
        return api.get(`/production-batches/${id}`)
    },
    create(data) {
        return api.post('/production-batches', data)
    }
}
