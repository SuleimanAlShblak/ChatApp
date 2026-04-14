<template>
  <div class="sidebar" :class="sidebarClass" :style="sidebarStyle">
    <div v-if="title" class="sidebar-header">
      <h2 class="sidebar-title">{{ title }}</h2>
      <button
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
      </button>
    </div>

    <div v-show="!isCollapsed" class="sidebar-content">
      <div class="sidebar-items">
        <component
          v-for="(item, index) in items"
          :key="item.id || index"
          :is="getComponentType(item)"
          v-bind="getComponentProps(item)"
          @click="handleItemClick(item, $event)"
          @action="handleItemAction(item, $event)"
        />
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
  type: 'card' | 'button' | 'divider' | 'spacer'
  // Card props
  title?: string
  description?: string
  icon?: any
  clickable?: boolean
  selected?: boolean
  actions?: any[]
  data?: any
  // Button props
  text?: string
  variant?: string
  size?: string
  // Common props
  class?: string
  disabled?: boolean
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
  itemClick: [item: SidebarItem, event?: Event]
  itemAction: [item: SidebarItem, action: any]
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

const getComponentType = (item: SidebarItem) => {
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

const getComponentProps = (item: SidebarItem) => {
  const { type, ...props } = item
  if (item.type === 'spacer') {
    return { 'data-type': 'spacer' }
  }
  return props
}

const handleItemClick = (item: SidebarItem, event?: Event) => {
  emit('itemClick', item, event)
}

const handleItemAction = (item: SidebarItem, action: any) => {
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
</style>
