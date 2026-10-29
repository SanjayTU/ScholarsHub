import { Alert, Badge, Button, Card, Col, Container, Row, Spinner } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

export default function MyProfile() {
  const { user, loading } = useAuth();

  if (loading || !user) {
    return (
      <div className="d-flex justify-content-center py-5">
        <Spinner animation="border" />
      </div>
    );
  }

  const memberSince = user.memberSince
    ? new Date(user.memberSince).toLocaleDateString('en-IN', { year: 'numeric', month: 'long', day: 'numeric' })
    : '—';

  return (
    <Container className="py-3" style={{ maxWidth: 800 }}>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">My profile</h1>
        <Button as={Link} to="/profile/edit" variant="primary">
          Edit profile
        </Button>
      </div>
      <Card>
        <Card.Body>
          <Row className="g-3">
            <Col md={3} className="text-center">
              {user.profileImagePath ? (
                <img
                  src={user.profileImagePath}
                  alt={user.fullName}
                  className="rounded-circle"
                  style={{ width: 96, height: 96, objectFit: 'cover' }}
                />
              ) : (
                <div
                  className="rounded-circle bg-secondary text-white d-inline-flex align-items-center justify-content-center"
                  style={{ width: 96, height: 96, fontSize: 32 }}
                >
                  {(user.fullName || '?')[0].toUpperCase()}
                </div>
              )}
            </Col>
            <Col md={9}>
              <h2 className="h5">{user.fullName}</h2>
              <p className="text-muted mb-2">Member since {memberSince}</p>
              <p className="mb-1"><strong>Education:</strong> {user.educationLevelName || '—'}</p>
              <p className="mb-1"><strong>Institution:</strong> {user.institution}</p>
              <p className="mb-1"><strong>Department:</strong> {user.departmentName || '—'}</p>
              <p className="mb-1"><strong>Semester / Class:</strong> {user.semesterOrClass}</p>
              <p className="mb-1"><strong>Locality:</strong> {user.locality}</p>
              <p className="mb-1"><strong>Email:</strong> {user.email}</p>
              <p className="mb-1"><strong>Mobile:</strong> {user.mobile}</p>
              <p className="mb-0">
                <strong>Call seller:</strong>{' '}
                {user.showPhone ? <Badge bg="success">Enabled</Badge> : <Badge bg="secondary">Hidden</Badge>}
              </p>
            </Col>
          </Row>
          <hr />
          <Row>
            <Col>
              <Alert variant="light" className="mb-0 border">
                Active listings: <strong>{user.activeListingsCount ?? 0}</strong>
              </Alert>
            </Col>
            <Col>
              <Alert variant="light" className="mb-0 border">
                Sold: <strong>{user.soldListingsCount ?? 0}</strong>
              </Alert>
            </Col>
          </Row>
        </Card.Body>
      </Card>
    </Container>
  );
}
