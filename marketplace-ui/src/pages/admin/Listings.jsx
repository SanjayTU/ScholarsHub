import { useCallback, useEffect, useState } from 'react';
import { Alert, Button, Container, Form, Nav, Spinner, Table } from 'react-bootstrap';
import { Link, NavLink } from 'react-router-dom';
import { getAdminListings, removeListing } from '../../api/adminApi';

function apiError(err, fallback) {
  return err.response?.data?.message || fallback;
}

export default function AdminListings() {
  const [data, setData] = useState({ items: [], page: 1, totalPages: 1 });
  const [q, setQ] = useState('');
  const [status, setStatus] = useState('');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [busyId, setBusyId] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const result = await getAdminListings({
        q: q || undefined,
        status: status || undefined,
        page,
        pageSize: 12,
      });
      setData({
        items: result.items || [],
        page: result.page || 1,
        totalPages: result.totalPages || 1,
      });
    } catch (err) {
      setError(apiError(err, 'Could not load listings.'));
    } finally {
      setLoading(false);
    }
  }, [q, status, page]);

  useEffect(() => { load(); }, [load]);

  const onRemove = async (id) => {
    if (!window.confirm('Remove this listing?')) return;
    setBusyId(id);
    setError('');
    try {
      await removeListing(id);
      await load();
    } catch (err) {
      setError(apiError(err, 'Could not remove listing.'));
    } finally {
      setBusyId(null);
    }
  };

  return (
    <Container className="py-3">
      <h1 className="h4 mb-3">Listings</h1>
      <Nav variant="pills" className="mb-3 flex-wrap">
        <Nav.Link as={NavLink} to="/admin" end>Dashboard</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/users">Users</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/listings">Listings</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/categories">Categories</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/reports">Reports</Nav.Link>
      </Nav>
      <Form className="d-flex flex-wrap gap-2 mb-3" onSubmit={(e) => { e.preventDefault(); setPage(1); load(); }}>
        <Form.Control
          placeholder="Search title or seller"
          value={q}
          onChange={(e) => setQ(e.target.value)}
          style={{ maxWidth: 280 }}
        />
        <Form.Select value={status} onChange={(e) => { setStatus(e.target.value); setPage(1); }} style={{ maxWidth: 180 }}>
          <option value="">All statuses</option>
          <option value="Available">Available</option>
          <option value="Sold">Sold</option>
          <option value="Unavailable">Unavailable</option>
          <option value="Removed">Removed</option>
        </Form.Select>
        <Button type="submit" variant="outline-primary">Search</Button>
      </Form>
      {error && <Alert variant="danger">{error}</Alert>}
      {loading ? (
        <div className="d-flex justify-content-center py-4"><Spinner animation="border" /></div>
      ) : (
        <Table responsive hover size="sm" className="align-middle">
          <thead>
            <tr>
              <th>Title</th>
              <th>Seller</th>
              <th>Category</th>
              <th>Price</th>
              <th>Status</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {data.items.length === 0 && (
              <tr><td colSpan={6} className="text-muted">No listings found.</td></tr>
            )}
            {data.items.map((l) => (
              <tr key={l.id}>
                <td><Link to={`/listings/${l.id}`}>{l.title}</Link></td>
                <td>{l.sellerName}</td>
                <td>{l.categoryName}</td>
                <td>₹{l.price}</td>
                <td>{l.status}</td>
                <td>
                  {l.status !== 'Removed' && (
                    <Button size="sm" variant="outline-danger" disabled={busyId === l.id} onClick={() => onRemove(l.id)}>
                      Remove
                    </Button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
      <div className="d-flex gap-2">
        <Button size="sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>Previous</Button>
        <Button size="sm" disabled={page >= data.totalPages} onClick={() => setPage((p) => p + 1)}>Next</Button>
      </div>
    </Container>
  );
}
