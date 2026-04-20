<script setup lang="ts">
import { ref, computed, watch } from 'vue'

interface SidebarItem {
  id?: string
  userName: string
  status?: string
  type?: string
  clickable?: boolean
  selected?: boolean
  data?: any
}

interface Props {
  items: SidebarItem[]
  title?: string
  collapsible?: boolean
  collapsed?: boolean
  position?: 'left' | 'right'
  width?: string
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  collapsible: false,
  collapsed: false,
  position: 'left',
  width: '300px',
})

const emit = defineEmits<{
  itemClick: [item: any, event?: Event]
  itemAction: [item: any, action: any]
  toggle: [collapsed: boolean]
}>()

const isCollapsed = ref(props.collapsed)

const sidebarClass = computed(() => {
  return [
    'sidebar',
    `sidebar--${props.position}`,
    {
      'sidebar--collapsed': isCollapsed.value,
    },
    props.class,
  ]
    .filter(Boolean)
    .join(' ')
})

const sidebarStyle = computed(() => {
  return {
    width: isCollapsed.value ? '60px' : props.width,
  }
})

watch(
  () => props.collapsed,
  (value) => {
    isCollapsed.value = value
  },
)

const handleItemClick = (item: any, event?: Event) => {
  emit('itemClick', item, event)
}

</script>

<template>
  <div class="sidebar" :class="sidebarClass" :style="sidebarStyle">
    <div v-show="!isCollapsed" class="sidebar-content">
      <div class="sidebar-items">
        <div
          v-for="item in items"
          :key="item.id"
          class="sidebar-user-card"
          :class="{ clickable: true }"
          @click="handleItemClick(item, $event)"
          tabindex="0"
          @keydown.enter="handleItemClick(item, $event)"
        >
          <div class="user-avatar">👤</div>
          <div class="user-info">
            <div class="user-name">{{item.userName }}</div>
            <div class="user-status" :class="item.status === 'online' ? 'online' : 'offline'">
              {{ item.status === 'online' ? 'Online' : 'Offline' }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>


<style>
</style>
