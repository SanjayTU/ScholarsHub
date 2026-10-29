import { useState } from 'react';
import { listingImageUrl } from '../api/listingApi';

export default function ImageGallery({ images }) {
  const urls = (images || []).slice(0, 5).map(listingImageUrl).filter(Boolean);
  const [active, setActive] = useState(0);
  const current = urls[Math.min(active, Math.max(urls.length - 1, 0))];

  if (urls.length === 0) {
    return (
      <div className="bg-light d-flex align-items-center justify-content-center text-muted rounded" style={{ height: 320 }}>
        No images
      </div>
    );
  }

  return (
    <div>
      <img src={current} alt="" className="w-100 rounded border mb-2" style={{ height: 360, objectFit: 'contain', background: '#f8f9fa' }} />
      {urls.length > 1 && (
        <div className="d-flex gap-2 flex-wrap">
          {urls.map((src, i) => (
            <button
              key={src}
              type="button"
              className={`p-0 border rounded ${i === active ? 'border-primary border-2' : ''}`}
              onClick={() => setActive(i)}
            >
              <img src={src} alt="" style={{ width: 64, height: 64, objectFit: 'cover', display: 'block' }} />
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
