import api from './api'

export default {
    getAll() {
        return api.get('/cash-accounts')
    },
    getSummary() {
        return api.get('/cash-accounts/summary')
    },
    getById(id) {
        return api.get(`/cash-accounts/${id}`)
    },
    create(data) {
        return api.post('/cash-accounts', data)
    }
}
