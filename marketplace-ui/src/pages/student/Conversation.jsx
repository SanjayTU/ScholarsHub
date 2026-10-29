import { useCallback, useEffect, useRef, useState } from 'react';
import { Alert, Button, Container, Form, Spinner } from 'react-bootstrap';
import { Link, useParams } from 'react-router-dom';
import { getConversations, getMessages, sendMessage } from '../../api/chatApi';
import { listingImageUrl } from '../../api/listingApi';
import { useAuth } from '../../context/AuthContext';

function apiError(err, fallback) {
  const data = err.response?.data;
  if (data?.errors) {
    const first = Object.values(data.errors).flat()[0];
    if (first) return first;
  }
  return data?.message || fallback;
}

export default function Conversation() {
  const { id } = useParams();
  const { user } = useAuth();
  const [meta, setMeta] = useState(null);
  const [messages, setMessages] = useState([]);
  const [body, setBody] = useState('');
  const [loading, setLoading] = useState(true);
  const [sending, setSending] = useState(false);
  const [error, setError] = useState('');
  const bottomRef = useRef(null);
  const lastIdRef = useRef(0);

  const scrollToBottom = () => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const mergeMessages = useCallback((incoming) => {
    if (!Array.isArray(incoming) || incoming.length === 0) return;
    setMessages((prev) => {
      const seen = new Set(prev.map((m) => m.id));
      const next = [...prev];
      incoming.forEach((m) => {
        if (!seen.has(m.id)) {
          next.push(m);
          seen.add(m.id);
        }
      });
      next.sort((a, b) => a.id - b.id);
      const last = next[next.length - 1];
      if (last) lastIdRef.current = last.id;
      return next;
    });
  }, []);

  useEffect(() => {
    let cancelled = false;
    lastIdRef.current = 0;
    setMessages([]);
    setMeta(null);
    (async () => {
      setLoading(true);
      setError('');
      try {
        const [inbox, list] = await Promise.all([
          getConversations(),
          getMessages(id),
        ]);
        if (cancelled) return;
        const found = (Array.isArray(inbox) ? inbox : []).find((c) => String(c.id) === String(id));
        setMeta(found || null);
        const msgs = Array.isArray(list) ? list : [];
        setMessages(msgs);
        const last = msgs[msgs.length - 1];
        lastIdRef.current = last ? last.id : 0;
      } catch (err) {
        if (!cancelled) setError(apiError(err, 'Could not load conversation.'));
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [id]);

  useEffect(() => {
    if (!loading) scrollToBottom();
  }, [messages, loading]);

  useEffect(() => {
    const timer = setInterval(async () => {
      try {
        const after = lastIdRef.current || undefined;
        const list = await getMessages(id, after);
        mergeMessages(list);
      } catch {
        /* keep polling */
      }
    }, 5000);
    return () => clearInterval(timer);
  }, [id, mergeMessages]);

  const onSend = async (e) => {
    e.preventDefault();
    const text = body.trim();
    if (!text || sending) return;
    setSending(true);
    setError('');
    try {
      const msg = await sendMessage(id, text);
      setBody('');
      mergeMessages([msg]);
    } catch (err) {
      setError(apiError(err, 'Could not send message.'));
    } finally {
      setSending(false);
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
    <Container className="py-3 d-flex flex-column" style={{ maxWidth: 720, height: 'calc(100vh - 120px)' }}>
      <div className="d-flex align-items-center gap-2 mb-2">
        <Button as={Link} to="/chats" variant="link" className="px-0">← Inbox</Button>
      </div>
      {meta && (
        <div className="d-flex align-items-center gap-2 mb-2">
          {meta.listingThumbnail && (
            <img
              src={listingImageUrl(meta.listingThumbnail)}
              alt=""
              className="rounded"
              style={{ width: 40, height: 40, objectFit: 'cover' }}
            />
          )}
          <div className="min-w-0">
            <div className="fw-semibold text-truncate">{meta.listingTitle}</div>
            <div className="small text-muted">{meta.otherParticipantName}</div>
          </div>
        </div>
      )}
      {error && <Alert variant="danger">{error}</Alert>}
      <div
        className="flex-grow-1 border rounded p-3 mb-2 overflow-auto bg-light"
        style={{ minHeight: 240 }}
      >
        {messages.length === 0 && <p className="text-muted mb-0">No messages yet. Say hello.</p>}
        {messages.map((m) => {
          const mine = user?.id === m.senderId;
          return (
            <div key={m.id} className={`d-flex mb-2 ${mine ? 'justify-content-end' : 'justify-content-start'}`}>
              <div
                className={`rounded px-3 py-2 ${mine ? 'bg-primary text-white' : 'bg-white border'}`}
                style={{ maxWidth: '80%' }}
              >
                <div style={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>{m.body}</div>
                <div className={`small ${mine ? 'text-white-50' : 'text-muted'}`}>
                  {new Date(m.sentAt).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' })}
                </div>
              </div>
            </div>
          );
        })}
        <div ref={bottomRef} />
      </div>
      <Form onSubmit={onSend} className="d-flex gap-2">
        <Form.Control
          as="textarea"
          rows={2}
          maxLength={1000}
          value={body}
          onChange={(e) => setBody(e.target.value)}
          placeholder="Type a message"
        />
        <Button type="submit" disabled={sending || !body.trim()}>
          {sending ? <Spinner size="sm" animation="border" /> : 'Send'}
        </Button>
      </Form>
      <div className="small text-muted mt-1">{body.length}/1000</div>
    </Container>
  );
}
