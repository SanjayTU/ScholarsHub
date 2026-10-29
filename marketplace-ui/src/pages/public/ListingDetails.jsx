import { useEffect, useState } from 'react';
import { Alert, Button, Card, Col, Container, Form, Modal, Row, Spinner } from 'react-bootstrap';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { startConversation } from '../../api/chatApi';
import { reportListing } from '../../api/reportApi';
import { getListing, getListingContact } from '../../api/publicApi';
import ImageGallery from '../../components/ImageGallery';
import StatusBadge from '../../components/StatusBadge';
import { useAuth } from '../../context/AuthContext';

function apiError(err, fallback) {
  const data = err.response?.data;
  if (data?.errors) {
    const first = Object.values(data.errors).flat()[0];
    if (first) return first;
  }
  return data?.message || fallback;
}

export default function ListingDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAuth();
  const [listing, setListing] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [callError, setCallError] = useState('');
  const [calling, setCalling] = useState(false);
  const [chatError, setChatError] = useState('');
  const [chatting, setChatting] = useState(false);
  const [showReport, setShowReport] = useState(false);
  const [reportReason, setReportReason] = useState('Misleading');
  const [reportComment, setReportComment] = useState('');
  const [reporting, setReporting] = useState(false);
  const [reportError, setReportError] = useState('');
  const [reportOk, setReportOk] = useState('');

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError('');
      try {
        const data = await getListing(id);
        if (!cancelled) setListing(data);
      } catch (err) {
        if (!cancelled) setError(apiError(err, 'Listing not found.'));
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [id]);

  const chatWithSeller = async () => {
    if (!isAuthenticated) {
      navigate('/login', { state: { from: { pathname: `/listings/${id}` } } });
      return;
    }
    setChatting(true);
    setChatError('');
    try {
      const conv = await startConversation(Number(id));
      navigate(`/chats/${conv.id}`);
    } catch (err) {
      setChatError(apiError(err, 'Could not start chat.'));
    } finally {
      setChatting(false);
    }
  };

  const submitReport = async (e) => {
    e.preventDefault();
    if (!isAuthenticated) {
      navigate('/login', { state: { from: { pathname: `/listings/${id}` } } });
      return;
    }
    setReporting(true);
    setReportError('');
    try {
      await reportListing(id, {
        reason: reportReason,
        comment: reportComment.trim() || null,
      });
      setShowReport(false);
      setReportComment('');
      setReportOk('Report submitted.');
    } catch (err) {
      setReportError(apiError(err, 'Could not submit report.'));
    } finally {
      setReporting(false);
    }
  };

  const callSeller = async () => {
    if (!isAuthenticated) {
      navigate('/login', { state: { from: { pathname: `/listings/${id}` } } });
      return;
    }
    setCalling(true);
    setCallError('');
    try {
      const contact = await getListingContact(id);
      if (contact?.mobile) window.location.href = `tel:${contact.mobile}`;
      else setCallError('Phone number is not available.');
    } catch (err) {
      setCallError(apiError(err, 'Could not get seller contact.'));
    } finally {
      setCalling(false);
    }
  };

  if (loading) {
    return (
      <div className="d-flex justify-content-center py-5">
        <Spinner animation="border" />
      </div>
    );
  }

  if (error || !listing) {
    return (
      <Container className="py-3" style={{ maxWidth: 800 }}>
        <Alert variant="danger">{error || 'Listing not found.'}</Alert>
        <Button as={Link} to="/listings" variant="outline-secondary">Back to listings</Button>
      </Container>
    );
  }

  const posted = listing.createdAt
    ? new Date(listing.createdAt).toLocaleDateString('en-IN', { day: 'numeric', month: 'long', year: 'numeric' })
    : '';
  const isOwner = user?.id === listing.seller?.id;
  const canCall = listing.status === 'Available' && listing.seller?.showPhone && !isOwner;
  const canChat = listing.status === 'Available' && !isOwner;
  const canReport = !isOwner;

  return (
    <Container className="py-3 py-md-4">
      <Button as={Link} to="/listings" variant="link" className="px-0 mb-3 text-decoration-none fw-semibold">← Back to listings</Button>
      <Row className="g-4 text-start align-items-start">
        <Col lg={8}>
          <div className="modern-card p-3 p-md-4">
            <ImageGallery images={listing.imageUrls} />
          </div>
          <Card className="modern-card mt-4">
            <Card.Body className="p-4">
              <h2 className="h5 fw-bold mb-3">About this item</h2>
              <p className="mb-0" style={{ whiteSpace: 'pre-wrap', color: 'var(--text-muted)' }}>{listing.description}</p>
            </Card.Body>
          </Card>
        </Col>
        <Col lg={4}>
          <Card className="listing-sidebar modern-card p-4">
            <Card.Body className="p-0">
              <div className="d-flex justify-content-between align-items-start gap-2 mb-3">
                <h1 className="h4 fw-bold mb-0">{listing.title}</h1>
                <StatusBadge status={listing.status} />
              </div>
              <p className="h2 fw-bold text-primary mb-4">₹{listing.price}</p>
              <div className="d-flex flex-column gap-2 border-top border-bottom py-3 mb-3 small">
                <p className="mb-0"><strong>Category:</strong> {listing.parentCategoryName ? `${listing.parentCategoryName} / ${listing.categoryName}` : listing.categoryName}</p>
                <p className="mb-0"><strong>Condition:</strong> {listing.condition}</p>
                <p className="mb-0"><strong>Education:</strong> {listing.educationLevelName}</p>
                <p className="mb-0"><strong>Department:</strong> {listing.departmentName || '—'}</p>
                <p className="mb-0"><strong>Semester / Class:</strong> {listing.semesterOrClass ?? '—'}</p>
                <p className="mb-0"><strong>Location:</strong> {listing.location}</p>
              </div>
              <p className="text-muted small mb-3">Posted {posted}</p>
              {(canChat || canCall || canReport) && (
                <div className="d-grid gap-2 mb-2">
                  {canChat && (
                    <Button onClick={chatWithSeller} disabled={chatting} size="lg" className="rounded-pill fw-semibold">
                      {chatting ? <Spinner size="sm" animation="border" /> : 'Chat with Seller'}
                    </Button>
                  )}
                  {canCall && (
                    <Button onClick={callSeller} disabled={calling} variant="outline-primary" size="lg" className="rounded-pill fw-semibold">
                      {calling ? <Spinner size="sm" animation="border" /> : 'Call Seller'}
                    </Button>
                  )}
                  {canReport && (
                    <Button
                      variant="link"
                      className="text-danger text-decoration-none"
                      onClick={() => {
                        if (!isAuthenticated) {
                          navigate('/login', { state: { from: { pathname: `/listings/${id}` } } });
                          return;
                        }
                        setReportError('');
                        setShowReport(true);
                      }}
                    >
                      Report Listing
                    </Button>
                  )}
                </div>
              )}
              {isOwner && listing.status !== 'Removed' && (
                <div className="d-flex flex-wrap gap-2 mb-2">
                  <Button as={Link} to={`/listings/${listing.id}/edit`} className="rounded-pill">Edit listing</Button>
                  <Button as={Link} to="/my-listings" className="rounded-pill" variant="outline-secondary">Manage</Button>
                </div>
              )}
              {(callError || chatError || reportError) && (
                <Alert variant="danger" className="mt-2">{reportError || chatError || callError}</Alert>
              )}
              {reportOk && <Alert variant="success" className="mt-2">{reportOk}</Alert>}
              <Card className="border-0 bg-light rounded-4 mt-4">
                <Card.Body>
                  <Card.Title className="h6 fw-bold mb-3">Seller information</Card.Title>
                  <p className="mb-1 fw-semibold">{listing.seller?.fullName}</p>
                  <p className="mb-1 small">{listing.seller?.institution}</p>
                  <p className="mb-0 small text-muted">{listing.seller?.locality}</p>
                </Card.Body>
              </Card>
          </Card.Body>
          </Card>
        </Col>
      </Row>

      <Modal show={showReport} onHide={() => setShowReport(false)}>
        <Form onSubmit={submitReport}>
          <Modal.Header closeButton>
            <Modal.Title>Report listing</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            {reportError && <Alert variant="danger">{reportError}</Alert>}
            <Form.Group className="mb-3">
              <Form.Label>Reason</Form.Label>
              <Form.Select value={reportReason} onChange={(e) => setReportReason(e.target.value)} required>
                <option value="Misleading">Misleading</option>
                <option value="ProhibitedItem">Prohibited Item</option>
                <option value="FraudScam">Fraud/Scam</option>
                <option value="WrongCategory">Wrong Category</option>
                <option value="Other">Other</option>
              </Form.Select>
            </Form.Group>
            <Form.Group>
              <Form.Label>Comment (optional)</Form.Label>
              <Form.Control
                as="textarea"
                rows={3}
                maxLength={500}
                value={reportComment}
                onChange={(e) => setReportComment(e.target.value)}
              />
            </Form.Group>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={() => setShowReport(false)}>Cancel</Button>
            <Button type="submit" variant="danger" disabled={reporting}>
              {reporting ? 'Submitting…' : 'Submit report'}
            </Button>
          </Modal.Footer>
        </Form>
      </Modal>
    </Container>
  );
}
