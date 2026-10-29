import { useEffect, useState } from 'react';
import { Alert, Card, Col, Container, Nav, Row, Spinner } from 'react-bootstrap';
import { NavLink } from 'react-router-dom';
import { getDashboard } from '../../api/adminApi';

function apiError(err, fallback) {
  return err.response?.data?.message || fallback;
}

export default function Dashboard() {
  const [stats, setStats] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError('');
      try {
        const data = await getDashboard();
        if (!cancelled) setStats(data);
      } catch (err) {
        if (!cancelled) setError(apiError(err, 'Could not load dashboard.'));
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, []);

  if (loading) {
    return (
      <div className="d-flex justify-content-center py-5">
        <Spinner animation="border" />
      </div>
    );
  }

  const cards = [
    { label: 'Total Users', value: stats?.totalUsers ?? 0 },
    { label: 'Active Listings', value: stats?.activeListings ?? 0 },
    { label: 'Sold Items', value: stats?.soldItems ?? 0 },
    { label: 'Pending Reports', value: stats?.pendingReports ?? 0 },
    { label: 'Blocked Users', value: stats?.blockedUsers ?? 0 },
  ];

  return (
    <Container className="py-3">
      <h1 className="h4 mb-3">Admin dashboard</h1>
      <Nav variant="pills" className="mb-3 flex-wrap">
        <Nav.Link as={NavLink} to="/admin" end>Dashboard</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/users">Users</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/listings">Listings</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/categories">Categories</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/reports">Reports</Nav.Link>
      </Nav>
      {error && <Alert variant="danger">{error}</Alert>}
      <Row className="g-3">
        {cards.map((c) => (
          <Col md={6} lg key={c.label}>
            <Card className="text-center h-100">
              <Card.Body>
                <div className="text-muted small">{c.label}</div>
                <div className="h3 mb-0">{c.value}</div>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>
    </Container>
  );
}
