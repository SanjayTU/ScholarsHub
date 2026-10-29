import { useCallback, useEffect, useState } from 'react';
import { Alert, Button, Container, Form, Modal, Nav, Spinner, Table } from 'react-bootstrap';
import { Link, NavLink } from 'react-router-dom';
import { getReports, handleReport } from '../../api/adminApi';

function apiError(err, fallback) {
  return err.response?.data?.message || fallback;
}

export default function Reports() {
  const [items, setItems] = useState([]);
  const [status, setStatus] = useState('Pending');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selected, setSelected] = useState(null);
  const [action, setAction] = useState('dismiss');
  const [adminNote, setAdminNote] = useState('');
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await getReports(status || undefined);
      setItems(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(apiError(err, 'Could not load reports.'));
    } finally {
      setLoading(false);
    }
  }, [status]);

  useEffect(() => { load(); }, [load]);

  const open = (report, act) => {
    setSelected(report);
    setAction(act);
    setAdminNote('');
  };

  const submit = async () => {
    if (!selected) return;
    setSaving(true);
    setError('');
    try {
      await handleReport(selected.id, { action, adminNote: adminNote.trim() || null });
      setSelected(null);
      await load();
    } catch (err) {
      setError(apiError(err, 'Could not handle report.'));
    } finally {
      setSaving(false);
    }
  };

  return (
    <Container className="py-3">
      <h1 className="h4 mb-3">Reports</h1>
      <Nav variant="pills" className="mb-3 flex-wrap">
        <Nav.Link as={NavLink} to="/admin" end>Dashboard</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/users">Users</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/listings">Listings</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/categories">Categories</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/reports">Reports</Nav.Link>
      </Nav>
      <Form.Select
        className="mb-3"
        style={{ maxWidth: 220 }}
        value={status}
        onChange={(e) => setStatus(e.target.value)}
      >
        <option value="Pending">Pending</option>
        <option value="Resolved">Resolved</option>
        <option value="Dismissed">Dismissed</option>
        <option value="">All</option>
      </Form.Select>
      {error && <Alert variant="danger">{error}</Alert>}
      {loading ? (
        <div className="d-flex justify-content-center py-4"><Spinner animation="border" /></div>
      ) : (
        <Table responsive hover size="sm" className="align-middle">
          <thead>
            <tr>
              <th>Listing</th>
              <th>Reporter</th>
              <th>Reason</th>
              <th>Comment</th>
              <th>Status</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {items.length === 0 && (
              <tr><td colSpan={6} className="text-muted">No reports.</td></tr>
            )}
            {items.map((r) => (
              <tr key={r.id}>
                <td><Link to={`/listings/${r.listingId}`}>{r.listingTitle}</Link></td>
                <td>{r.reporterName}</td>
                <td>{r.reason}</td>
                <td className="small">{r.comment || '—'}</td>
                <td>{r.status}</td>
                <td className="text-nowrap">
                  {r.status === 'Pending' && (
                    <>
                      <Button size="sm" variant="outline-secondary" className="me-1" onClick={() => open(r, 'dismiss')}>Dismiss</Button>
                      <Button size="sm" variant="outline-danger" className="me-1" onClick={() => open(r, 'removeListing')}>Remove Listing</Button>
                      <Button size="sm" variant="outline-warning" onClick={() => open(r, 'disableSeller')}>Disable Seller</Button>
                    </>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}

      <Modal show={!!selected} onHide={() => setSelected(null)}>
        <Modal.Header closeButton>
          <Modal.Title>Handle report</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p className="mb-2"><strong>Action:</strong> {action}</p>
          <p className="small text-muted">{selected?.listingTitle} — {selected?.reason}</p>
          <Form.Group>
            <Form.Label>Admin note (optional)</Form.Label>
            <Form.Control
              as="textarea"
              rows={3}
              maxLength={500}
              value={adminNote}
              onChange={(e) => setAdminNote(e.target.value)}
            />
          </Form.Group>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setSelected(null)}>Cancel</Button>
          <Button variant="primary" disabled={saving} onClick={submit}>
            {saving ? 'Saving…' : 'Confirm'}
          </Button>
        </Modal.Footer>
      </Modal>
    </Container>
  );
}
