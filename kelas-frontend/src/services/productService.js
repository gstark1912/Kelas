import api from './api'

export default {
    getAll(filters = {}) {
        return api.get('/products', { params: filters })
    },
    getById(id) {
        return api.get(`/products/${id}`)
    },
    create(data) {
        return api.post('/products', data)
    },
    update(id, data) {
        return api.put(`/products/${id}`, data)
    },
    updateRecipe(id, ingredients) {
        return api.put(`/products/${id}/recipe`, { ingredients })
    },
    updateVisibility(id, isVisible) {
        return api.patch(`/products/${id}/visibility`, { isVisible })
    }
}
