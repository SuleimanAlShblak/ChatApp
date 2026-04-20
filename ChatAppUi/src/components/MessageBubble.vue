<template>
  <div :class="['message-bubble', { 'message-bubble--own': isOwn }]">
    <div class="message-meta">
      <span class="message-sender">{{ sender }}</span>
      <span class="message-time">{{ time }}</span>
    </div>
    <div class="message-content">{{ message }}</div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  sender: string
  message: string
  time?: string
  isOwn?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  time: '',
  isOwn: false,
})
</script>

<style >
</style>
