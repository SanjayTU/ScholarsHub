import { useEffect, useState } from 'react';
import { Alert, Badge, Card, Container, Spinner } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import { getConversations } from '../../api/chatApi';
import { listingImageUrl } from '../../api/listingApi';

function apiError(err, fallback) {
  const data = err.response?.data;
  if (data?.errors) {
    const first = Object.values(data.errors).flat()[0];
    if (first) return first;
  }
  return data?.message || fallback;
}

export default function Inbox() {
  const [conversations, setConversations] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError('');
      try {
        const data = await getConversations();
        if (!cancelled) setConversations(Array.isArray(data) ? data : []);
      } catch (err) {
        if (!cancelled) setError(apiError(err, 'Could not load conversations.'));
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

  return (
    <Container className="py-3" style={{ maxWidth: 720 }}>
      <h1 className="h4 mb-3">Inbox</h1>
      {error && <Alert variant="danger">{error}</Alert>}
      {conversations.length === 0 && !error && (
        <Alert variant="light" className="border">No conversations yet.</Alert>
      )}
      <div className="d-flex flex-column gap-2">
        {conversations.map((c) => {
          const time = c.lastMessageAt
            ? new Date(c.lastMessageAt).toLocaleString('en-IN', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' })
            : '';
          return (
            <Card as={Link} to={`/chats/${c.id}`} key={c.id} className="text-decoration-none text-reset">
              <Card.Body className="d-flex gap-3 align-items-center py-2">
                {c.listingThumbnail ? (
                  <img
                    src={listingImageUrl(c.listingThumbnail)}
                    alt=""
                    className="rounded"
                    style={{ width: 64, height: 64, objectFit: 'cover', flexShrink: 0 }}
                  />
                ) : (
                  <div className="bg-light rounded" style={{ width: 64, height: 64, flexShrink: 0 }} />
                )}
                <div className="flex-grow-1 min-w-0">
                  <div className="d-flex justify-content-between gap-2">
                    <span className="fw-semibold text-truncate">{c.listingTitle}</span>
                    {c.unreadCount > 0 && <Badge bg="danger">{c.unreadCount}</Badge>}
                  </div>
                  <div className="small text-muted">{c.otherParticipantName}</div>
                  <div className="small text-truncate">{c.lastMessageBody || 'No messages yet'}</div>
                  <div className="small text-muted">{time}</div>
                </div>
              </Card.Body>
            </Card>
          );
        })}
      </div>
    </Container>
  );
}
