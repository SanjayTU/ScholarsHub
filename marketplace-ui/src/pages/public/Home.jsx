import { useEffect, useState } from 'react';
import { Alert, Button, Card, Col, Container, Form, Row, Spinner } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { searchListings } from '../../api/publicApi';
import ListingCard from '../../components/ListingCard';
import { useAuth } from '../../context/AuthContext';

export default function Home() {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [query, setQuery] = useState('');
  const [listings, setListings] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let cancelled = false;

    async function loadListings() {
      try {
        const result = await searchListings({ page: 1, pageSize: 4, sort: 'newest' });
        if (!cancelled) setListings(result.items || []);
      } catch {
        if (!cancelled) setError('Could not load recent listings. You can still browse all listings.');
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    loadListings();
    return () => {
      cancelled = true;
    };
  }, []);

  const search = (event) => {
    event.preventDefault();
    const params = new URLSearchParams();
    if (query.trim()) params.set('q', query.trim());
    navigate(`/listings${params.size ? `?${params}` : ''}`);
  };

  return (
    <Container className="py-3 py-md-4">
      <section className="home-hero rounded-4 p-4 p-md-5 mb-4">
        <Row className="align-items-center g-4">
          <Col lg={7}>
            <p className="text-uppercase fw-semibold small mb-2">Your campus marketplace</p>
            <h1 className="display-5 fw-bold mb-3">Find what you need. Pass on what you don’t.</h1>
            <p className="lead mb-4">
              Buy and sell books, study materials, stationery, and more with students in your community.
            </p>
            <Form onSubmit={search} className="d-flex flex-column flex-sm-row gap-2">
              <Form.Control
                aria-label="Search marketplace"
                placeholder="Search books, calculators, notes..."
                value={query}
                onChange={(event) => setQuery(event.target.value)}
              />
              <Button type="submit" variant="light" className="text-primary fw-semibold px-4">
                Search listings
              </Button>
            </Form>
          </Col>
          <Col lg={5} className="text-lg-end">
            <div className="home-hero-actions d-flex flex-column flex-sm-row flex-lg-column gap-2 justify-content-lg-end">
              <Button as={Link} to="/listings" variant="light" size="lg">
                Browse all listings
              </Button>
              <Button as={Link} to={isAuthenticated ? '/sell' : '/register'} variant="outline-light" size="lg">
                {isAuthenticated ? 'Sell an item' : 'Join and sell an item'}
              </Button>
            </div>
          </Col>
        </Row>
      </section>

      <section className="mb-5">
        <div className="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
          <div>
            <h2 className="h4 mb-1">Recently listed</h2>
            <p className="text-muted mb-0">See what students are offering right now.</p>
          </div>
          <Button as={Link} to="/listings" variant="outline-primary">
            Explore all listings
          </Button>
        </div>

        {error && <Alert variant="warning">{error}</Alert>}
        {loading ? (
          <div className="d-flex justify-content-center py-5">
            <Spinner animation="border" role="status" aria-label="Loading recent listings" />
          </div>
        ) : listings.length > 0 ? (
          <Row className="g-3">
            {listings.map((listing) => (
              <Col sm={6} lg={3} key={listing.id}>
                <ListingCard listing={listing} />
              </Col>
            ))}
          </Row>
        ) : (
          !error && (
            <Card className="border-0 bg-light">
              <Card.Body className="py-4 text-center">
                <h3 className="h5">No listings yet</h3>
                <p className="text-muted mb-3">Be the first to share something useful with your community.</p>
                <Button as={Link} to={isAuthenticated ? '/sell' : '/register'}>
                  {isAuthenticated ? 'Create a listing' : 'Get started'}
                </Button>
              </Card.Body>
            </Card>
          )
        )}
      </section>

      <section className="mb-4">
        <h2 className="h4 mb-3">Marketplace shortcuts</h2>
        <Row className="g-3">
          <Col md={4}>
            <Card className="h-100">
              <Card.Body>
                <h3 className="h5">Browse and filter</h3>
                <p className="text-muted">Search by category, education, price, or location to find the right item.</p>
                <Button as={Link} to="/listings" variant="outline-primary">Browse listings</Button>
              </Card.Body>
            </Card>
          </Col>
          <Col md={4}>
            <Card className="h-100">
              <Card.Body>
                <h3 className="h5">Manage your items</h3>
                <p className="text-muted">Create listings and update availability as your items find a new home.</p>
                <Button as={Link} to={isAuthenticated ? '/my-listings' : '/login'} variant="outline-primary">
                  {isAuthenticated ? 'My listings' : 'Log in to sell'}
                </Button>
              </Card.Body>
            </Card>
          </Col>
          <Col md={4}>
            <Card className="h-100">
              <Card.Body>
                <h3 className="h5">Chat with students</h3>
                <p className="text-muted">Use your inbox to discuss an item directly with its seller.</p>
                <Button as={Link} to={isAuthenticated ? '/chats' : '/login'} variant="outline-primary">
                  {isAuthenticated ? 'Open inbox' : 'Log in to chat'}
                </Button>
              </Card.Body>
            </Card>
          </Col>
        </Row>
      </section>
    </Container>
  );
}
