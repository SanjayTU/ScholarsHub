import { useCallback, useEffect, useState } from 'react';
import { Alert, Button, Card, Col, Container, Row, Spinner } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { changeListingStatus, deleteListing, getMyListings, listingImageUrl } from '../../api/listingApi';
import StatusBadge from '../../components/StatusBadge';

function apiError(err, fallback) {
  const data = err.response?.data;
  if (data?.errors) {
    const first = Object.values(data.errors).flat()[0];
    if (first) return first;
  }
  return data?.message || fallback;
}

export default function MyListings() {
  const [listings, setListings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [busyId, setBusyId] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await getMyListings();
      setListings(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(apiError(err, 'Could not load listings.'));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const run = async (id, fn) => {
    setBusyId(id);
    setError('');
    try {
      await fn();
      await load();
    } catch (err) {
      setError(apiError(err, 'Action failed.'));
    } finally {
      setBusyId(null);
    }
  };

  const onDelete = (id) => {
    if (!window.confirm('Delete this listing?')) return;
    run(id, () => deleteListing(id));
  };

  if (loading && listings.length === 0) {
    return (
      <div className="d-flex justify-content-center py-5">
        <Spinner animation="border" />
      </div>
    );
  }

  return (
    <Container className="py-3">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">My listings</h1>
        <Button as={Link} to="/sell" variant="primary" size="sm">Sell an item</Button>
      </div>
      {error && <Alert variant="danger">{error}</Alert>}
      {listings.length === 0 && <Alert variant="light" className="border">You have no listings yet.</Alert>}
      <Row className="g-3">
        {listings.map((item) => {
          const cover = item.imageUrls?.[0];
          const busy = busyId === item.id;
          const canManage = item.status !== 'Removed';
          return (
            <Col md={6} lg={4} key={item.id}>
              <Card className="h-100 text-start">
                {cover && (
                  <Card.Img variant="top" src={listingImageUrl(cover)} alt={item.title} style={{ height: 180, objectFit: 'cover' }} />
                )}
                <Card.Body>
                  <div className="d-flex justify-content-between align-items-start gap-2">
                    <Card.Title className="h6 mb-1">{item.title}</Card.Title>
                    <StatusBadge status={item.status} />
                  </div>
                  <p className="mb-1 fw-semibold">₹{item.price}</p>
                  <p className="text-muted small mb-3">{item.categoryName} · {item.location}</p>
                  {canManage && (
                    <div className="d-flex flex-wrap gap-2">
                      <Button as={Link} to={`/listings/${item.id}/edit`} size="sm" variant="outline-primary" disabled={busy}>Edit</Button>
                      <Button size="sm" variant="outline-danger" disabled={busy} onClick={() => onDelete(item.id)}>Delete</Button>
                      {item.status === 'Available' && (
                        <>
                          <Button size="sm" variant="outline-secondary" disabled={busy} onClick={() => run(item.id, () => changeListingStatus(item.id, 'Sold'))}>Mark Sold</Button>
                          <Button size="sm" variant="outline-warning" disabled={busy} onClick={() => run(item.id, () => changeListingStatus(item.id, 'Unavailable'))}>Mark Unavailable</Button>
                        </>
                      )}
                      {(item.status === 'Sold' || item.status === 'Unavailable') && (
                        <Button size="sm" variant="outline-success" disabled={busy} onClick={() => run(item.id, () => changeListingStatus(item.id, 'Available'))}>Mark Available</Button>
                      )}
                    </div>
                  )}
                </Card.Body>
              </Card>
            </Col>
          );
        })}
      </Row>
    </Container>
  );
}
