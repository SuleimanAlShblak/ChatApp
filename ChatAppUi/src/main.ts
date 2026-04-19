import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import './assets/main.css'

import { library } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

import { faUserSecret, faPaperPlane } from '@fortawesome/free-solid-svg-icons'

library.add(faUserSecret, faPaperPlane)

createApp(App).use(router).component('font-awesome-icon', FontAwesomeIcon).mount('#app')
