<template>
  <div
    class="card"
    :class="[cardClass, { 'card--clickable': clickable, 'card--selected': selected }]"
    @click="handleClick"
  >
    <div v-if="icon || title" class="card-header">
      <div v-if="icon" class="card-icon">
        <template v-if="typeof icon === 'string'">
          <span class="card-icon-text">{{ icon }}</span>
        </template>
        <template v-else>
          <component :is="icon" />
        </template>
      </div>
      <h3 v-if="title" class="card-title">{{ title }}</h3>
    </div>

    <div v-if="description || $slots.default" class="card-content">
      <p v-if="description" class="card-description">{{ description }}</p>
      <slot />
    </div>

    <div v-if="actions && actions.length" class="card-actions">
      <button
        v-for="action in actions"
        :key="action.label"
        class="card-action-btn"
        :class="action.class"
        @click.stop="handleAction(action)"
      >
        {{ action.label }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Action {
  label: string
  action: string
  class?: string
}

interface Props {
  title?: string
  description?: string
  icon?: any
  clickable?: boolean
  selected?: boolean
  class?: string
  actions?: Action[]
  data?: any
}

const props = withDefaults(defineProps<Props>(), {
  clickable: false,
  selected: false,
})

const emit = defineEmits<{
  click: [data?: any]
  action: [action: Action, data?: any]
}>()

const cardClass = computed(() => {
  return ['card', props.class].filter(Boolean).join(' ')
})

const handleClick = () => {
  if (props.clickable) {
    emit('click', props.data)
  }
}

const handleAction = (action: Action) => {
  emit('action', action, props.data)
}
</script>

<style scoped>
.card {
  background: white;
  border-radius: 0.5rem;
  box-shadow:
    0 1px 3px 0 rgba(0, 0, 0, 0.1),
    0 1px 2px 0 rgba(0, 0, 0, 0.06);
  padding: 1rem;
  transition: all 0.2s ease-in-out;
  border: 1px solid #e5e7eb;
}

.card--clickable {
  cursor: pointer;
}

.card--clickable:hover {
  transform: translateY(-2px);
  box-shadow:
    0 4px 6px -1px rgba(0, 0, 0, 0.1),
    0 2px 4px -1px rgba(0, 0, 0, 0.06);
}

.card--selected {
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.card-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}

.card-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  background-color: #f3f4f6;
  border-radius: 0.375rem;
  color: #6b7280;
}

.card-icon-text {
  font-size: 1rem;
  font-weight: 700;
}

.card-title {
  font-size: 1rem;
  font-weight: 600;
  color: #111827;
  margin: 0;
}

.card-content {
  margin-bottom: 0.75rem;
}

.card-description {
  color: #6b7280;
  font-size: 0.875rem;
  line-height: 1.5;
  margin: 0;
}

.card-actions {
  display: flex;
  gap: 0.5rem;
  padding-top: 0.75rem;
  border-top: 1px solid #e5e7eb;
}

.card-action-btn {
  padding: 0.375rem 0.75rem;
  font-size: 0.875rem;
  font-weight: 500;
  border-radius: 0.375rem;
  border: 1px solid #d1d5db;
  background: white;
  color: #374151;
  cursor: pointer;
  transition: all 0.15s ease-in-out;
}

.card-action-btn:hover {
  background-color: #f9fafb;
  border-color: #9ca3af;
}

.card-action-btn:active {
  background-color: #f3f4f6;
}
</style>
