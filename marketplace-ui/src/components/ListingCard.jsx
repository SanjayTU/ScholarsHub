import { Card } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { listingImageUrl } from '../api/listingApi';

export default function ListingCard({ listing }) {
  const cover = listing?.coverImageUrl;
  const posted = listing?.createdAt
    ? new Date(listing.createdAt).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' })
    : '';

  return (
    <Card as={Link} to={`/listings/${listing.id}`} className="modern-card h-100 overflow-hidden text-decoration-none text-reset text-start">
      {cover ? (
        <Card.Img variant="top" src={listingImageUrl(cover)} alt={listing.title} style={{ height: 220, objectFit: 'cover', borderTopLeftRadius: 16, borderTopRightRadius: 16 }} />
      ) : (
        <div className="bg-light d-flex align-items-center justify-content-center text-muted" style={{ height: 220 }}>No image available</div>
      )}
      <Card.Body className="d-flex flex-column p-3 p-lg-4">
        <div className="d-flex align-items-start justify-content-between gap-2 mb-2">
          <Card.Title className="h6 fw-semibold mb-0">{listing.title}</Card.Title>
          <span className="fw-bold text-nowrap" style={{ color: 'var(--brand-primary)', fontWeight: 700 }}>₹{listing.price}</span>
        </div>
        <p className="text-muted small mb-3">{listing.condition}</p>
        <div className="mt-auto d-flex flex-wrap justify-content-between gap-2 text-muted small">
          <span>{listing.location}</span>
          {posted && <span>{posted}</span>}
        </div>
      </Card.Body>
    </Card>
  );
}
