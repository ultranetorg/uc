export const urlRegexMap = {
  video: /\.(mp4|webm|ogg)$/i,
  youtube: /(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/shorts\/)([\w-]{11})/,
  vkVideo: /(?:https?:\/\/)?(?:www\.)?(?:vk\.com|vkvideo\.ru)\/.*video.*/i,
}

export const getVideoType = (url: string) => {
  if (urlRegexMap.video.test(url)) return "video"
  if (urlRegexMap.youtube.test(url)) return "youtube"
  if (urlRegexMap.vkVideo.test(url)) return "vkVideo"

  return null
}
