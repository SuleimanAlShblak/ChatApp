<template>
  <header class="topbar" :class="topbarClass">
    <div class="topbar-left">
      <button
        v-if="showMenuButton"
        class="topbar-menu-btn"
        @click="$emit('menuClick')"
        aria-label="Toggle menu"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M4 6h16M4 12h16M4 18h16"
          />
        </svg>
      </button>

      <div v-if="title || $slots.title" class="topbar-title">
        <slot name="title">
          <h1 class="topbar-title-text">{{ title }}</h1>
        </slot>
      </div>
    </div>

    <div class="topbar-center">
      <slot name="center" />
    </div>

    <div class="topbar-right">
      <slot name="actions" />
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  title?: string
  showMenuButton?: boolean
  sticky?: boolean
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  showMenuButton: false,
  sticky: false,
})

const emit = defineEmits<{
  menuClick: []
}>()

const topbarClass = computed(() => {
  return [
    'topbar',
    {
      'topbar--sticky': props.sticky,
    },
    props.class,
  ]
    .filter(Boolean)
    .join(' ')
})
</script>

<style scoped>
.topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 4rem;
  padding: 0 1.5rem;
  background: white;
  border-bottom: 1px solid #e5e7eb;
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1);
}

.topbar--sticky {
  position: sticky;
  top: 0;
  z-index: 40;
}

.topbar-left,
.topbar-center,
.topbar-right {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.topbar-left {
  flex: 1;
}

.topbar-center {
  flex: 1;
  justify-content: center;
}

.topbar-right {
  flex: 1;
  justify-content: flex-end;
}

.topbar-menu-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 2.5rem;
  height: 2.5rem;
  border: none;
  background: transparent;
  color: #6b7280;
  cursor: pointer;
  border-radius: 0.375rem;
  transition: all 0.15s ease-in-out;
}

.topbar-menu-btn:hover {
  background: #f3f4f6;
  color: #374151;
}

.topbar-title {
  display: flex;
  align-items: center;
}

.topbar-title-text {
  font-size: 1.25rem;
  font-weight: 600;
  color: #111827;
  margin: 0;
}

/* Mobile responsive */
@media (max-width: 768px) {
  .topbar {
    padding: 0 1rem;
    height: 3.5rem;
  }

  .topbar-title-text {
    font-size: 1.125rem;
  }

  .topbar-left .topbar-title {
    display: none;
  }
}
</style>
