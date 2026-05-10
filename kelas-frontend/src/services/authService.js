import api from './api'

export default {
    login(email, password) {
        return api.post('/auth/login', { email, password })
    },
    me() {
        return api.get('/auth/me')
    }
}
