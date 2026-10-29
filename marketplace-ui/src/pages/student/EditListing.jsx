import { useEffect, useState } from 'react';
import { Alert, Button, Card, Container, Spinner } from 'react-bootstrap';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { getMyListings, updateListing } from '../../api/listingApi';
import { ListingForm } from './CreateListing';

function apiError(err, fallback) {
  const data = err.response?.data;
  if (data?.errors) {
    const first = Object.values(data.errors).flat()[0];
    if (first) return first;
  }
  return data?.message || fallback;
}

export default function EditListing() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [listing, setListing] = useState(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const list = await getMyListings();
        const found = (list || []).find((l) => String(l.id) === String(id));
        if (!cancelled) {
          if (!found) setLoadError('Listing not found.');
          else setListing(found);
        }
      } catch (err) {
        if (!cancelled) setLoadError(apiError(err, 'Could not load listing.'));
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [id]);

  const handleSubmit = async (payload) => {
    setSubmitting(true);
    setError('');
    try {
      await updateListing(id, payload);
      navigate('/my-listings');
    } catch (err) {
      setError(apiError(err, 'Could not update listing.'));
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="d-flex justify-content-center py-5">
        <Spinner animation="border" />
      </div>
    );
  }

  return (
    <Container className="py-3" style={{ maxWidth: 800 }}>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">Edit listing</h1>
        <Button as={Link} to="/my-listings" variant="outline-secondary" size="sm">Back</Button>
      </div>
      {loadError && <Alert variant="danger">{loadError}</Alert>}
      {listing?.status === 'Removed' && (
        <Alert variant="danger">Removed listings cannot be edited.</Alert>
      )}
      {listing && listing.status !== 'Removed' && (
        <Card>
          <Card.Body>
            <ListingForm
              mode="edit"
              initial={listing}
              existingImageUrls={listing.imageUrls}
              onSubmit={handleSubmit}
              submitting={submitting}
              error={error}
            />
          </Card.Body>
        </Card>
      )}
    </Container>
  );
}
