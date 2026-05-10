import api from './api'

export default {
    create(data) {
        return api.post('/stock-adjustments', data)
    },
    getMovements(itemType, itemId) {
        return api.get('/stock-movements', { params: { itemType, itemId } })
    }
}
