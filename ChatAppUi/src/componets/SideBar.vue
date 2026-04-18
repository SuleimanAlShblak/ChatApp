<template>
  <div class="sidebar" :class="sidebarClass" :style="sidebarStyle">
    <div v-if="title" class="sidebar-header">
      <!-- <h2 class="sidebar-title">{{ title }}</h2> -->
      <!-- <button
        v-if="collapsible"
        class="sidebar-toggle"
        @click="toggleCollapsed"
        :aria-label="isCollapsed ? 'Expand sidebar' : 'Collapse sidebar'"
      >
        <svg
          :class="{ 'rotate-180': isCollapsed }"
          class="w-4 h-4 transition-transform"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M15 19l-7-7 7-7"
          />
        </svg>
      </button> -->
    </div>

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

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Card from './Card.vue'
import Button from './Button.vue'

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

const toggleCollapsed = () => {
  isCollapsed.value = !isCollapsed.value
  emit('toggle', isCollapsed.value)
}

watch(
  () => props.collapsed,
  (value) => {
    isCollapsed.value = value
  },
)

const getComponentType = (item: any) => {
  switch (item.type) {
    case 'card':
      return Card
    case 'button':
      return Button
    case 'divider':
      return 'hr'
    case 'spacer':
      return 'div'
    default:
      return 'div'
  }
}

const getComponentProps = (item: any) => {
  const { type, ...props } = item
  if (item.type === 'spacer') {
    return { 'data-type': 'spacer' }
  }
  return props
}

const handleItemClick = (item: any, event?: Event) => {
  emit('itemClick', item, event)
}

const handleItemAction = (item: any, action: any) => {
  emit('itemAction', item, action)
}
</script>

<style scoped>
.sidebar {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: white;
  border-right: 1px solid #e5e7eb;
  transition: width 0.3s ease-in-out;
}

.sidebar--right {
  border-right: none;
  border-left: 1px solid #e5e7eb;
}

.sidebar--collapsed {
  width: 60px !important;
}

.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
  background: #f9fafb;
}

.sidebar-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: #111827;
  margin: 0;
}

.sidebar-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border: none;
  background: transparent;
  color: #6b7280;
  cursor: pointer;
  border-radius: 0.375rem;
  transition: all 0.15s ease-in-out;
}

.sidebar-toggle:hover {
  background: #e5e7eb;
  color: #374151;
}

.sidebar-content {
  flex: 1;
  overflow-y: auto;
  padding: 1rem;
}

.sidebar-items {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

/* Divider styles */
.sidebar-items hr {
  border: none;
  border-top: 1px solid #e5e7eb;
  margin: 0.5rem 0;
}

/* Spacer styles */
.sidebar-items > div[data-type='spacer'] {
  height: 1rem;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .sidebar {
    position: fixed;
    top: 0;
    left: 0;
    width: 280px;
    z-index: 50;
    transform: translateX(-100%);
    transition: transform 0.3s ease-in-out;
  }

  .sidebar--open {
    transform: translateX(0);
  }
}
/* User card styles for clickable chat selection */
.sidebar-user-card {
  display: flex;
  align-items: center;
  padding: 0.75rem 1rem;
  border-bottom: 1px solid #f0f0f0;
  cursor: pointer;
  transition: background 0.2s;
}
.sidebar-user-card:last-child {
  border-bottom: none;
}
.sidebar-user-card.clickable:hover,
.sidebar-user-card.clickable:focus {
  background: #f3f4f6;
}
.user-avatar {
  font-size: 1.5rem;
  margin-right: 0.75rem;
}
.user-info {
  flex: 1;
}
.user-name {
  font-weight: 600;
  color: #222;
}
.user-status {
  font-size: 0.9rem;
  color: #888;
}
.user-status.online {
  color: #22c55e;
}
.user-status.offline {
  color: #888;
}
</style>
