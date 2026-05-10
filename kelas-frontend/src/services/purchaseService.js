import api from './api'

export default {
    create(data) {
        return api.post('/purchases', data)
    },
    getByRawMaterial(rawMaterialId) {
        return api.get('/purchases', { params: { rawMaterialId } })
    }
}
