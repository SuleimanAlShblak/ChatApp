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

<style>
</style>
