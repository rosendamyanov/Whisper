import { useEffect, useRef } from 'react'

export const useStream = (
  streamSource: MediaStream | null
): React.RefObject<HTMLVideoElement | null> => {
  const videoRef = useRef<HTMLVideoElement>(null)

  useEffect(() => {
    const videoEl = videoRef.current
    if (videoEl && streamSource) {
      videoEl.srcObject = streamSource

      videoEl.play().catch((e) => {
        console.warn('Autoplay prevented:', e)
      })
    } else if (videoEl) {
      videoEl.srcObject = null
    }
  }, [streamSource])

  return videoRef
}
