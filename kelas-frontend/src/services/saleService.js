import api from './api'

export default {
    getAll(filters = {}) {
        return api.get('/sales', { params: filters })
    },
    getById(id) {
        return api.get(`/sales/${id}`)
    },
    create(data) {
        return api.post('/sales', data)
    }
}
