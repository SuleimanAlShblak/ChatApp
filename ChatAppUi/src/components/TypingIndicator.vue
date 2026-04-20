<template>
  <div class="typing-indicator" aria-live="polite" aria-atomic="true">
    <div class="typing-indicator__bubble">
      <span class="typing-indicator__text">{{ name }} is typing</span>
      <span class="typing-indicator__dots" aria-hidden="true">
        <span></span>
        <span></span>
        <span></span>
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  name?: string
}

withDefaults(defineProps<Props>(), {
  name: 'Someone',
})
</script>

<style>
</style>
