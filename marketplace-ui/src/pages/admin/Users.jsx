import { useCallback, useEffect, useState } from 'react';
import { Alert, Button, Container, Form, Nav, Spinner, Table } from 'react-bootstrap';
import { NavLink } from 'react-router-dom';
import { deleteUser, getUsers, toggleUserStatus } from '../../api/adminApi';
import { useAuth } from '../../context/AuthContext';

function apiError(err, fallback) {
  return err.response?.data?.message || fallback;
}

export default function Users() {
  const { user: me } = useAuth();
  const [data, setData] = useState({ items: [], page: 1, totalPages: 1 });
  const [q, setQ] = useState('');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [busyId, setBusyId] = useState(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const result = await getUsers({ q: q || undefined, page, pageSize: 12 });
      setData({
        items: result.items || [],
        page: result.page || 1,
        totalPages: result.totalPages || 1,
      });
    } catch (err) {
      setError(apiError(err, 'Could not load users.'));
    } finally {
      setLoading(false);
    }
  }, [q, page]);

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

  return (
    <Container className="py-3">
      <h1 className="h4 mb-3">Users</h1>
      <Nav variant="pills" className="mb-3 flex-wrap">
        <Nav.Link as={NavLink} to="/admin" end>Dashboard</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/users">Users</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/listings">Listings</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/categories">Categories</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/reports">Reports</Nav.Link>
      </Nav>
      <Form className="d-flex gap-2 mb-3" onSubmit={(e) => { e.preventDefault(); setPage(1); load(); }}>
        <Form.Control
          placeholder="Search name, email, mobile"
          value={q}
          onChange={(e) => setQ(e.target.value)}
        />
        <Button type="submit" variant="outline-primary">Search</Button>
      </Form>
      {error && <Alert variant="danger">{error}</Alert>}
      {loading ? (
        <div className="d-flex justify-content-center py-4"><Spinner animation="border" /></div>
      ) : (
        <Table responsive hover size="sm" className="align-middle">
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Mobile</th>
              <th>Role</th>
              <th>Status</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {data.items.length === 0 && (
              <tr><td colSpan={6} className="text-muted">No users found.</td></tr>
            )}
            {data.items.map((u) => {
              const self = me?.id === u.id;
              const busy = busyId === u.id;
              return (
                <tr key={u.id}>
                  <td>{u.fullName}</td>
                  <td>{u.email}</td>
                  <td>{u.mobile}</td>
                  <td>{u.role}</td>
                  <td>{u.status}{u.isDeleted ? ' (deleted)' : ''}</td>
                  <td className="text-nowrap">
                    {!self && !u.isDeleted && (
                      <>
                        <Button
                          size="sm"
                          variant={u.status === 'Disabled' ? 'outline-success' : 'outline-warning'}
                          className="me-1"
                          disabled={busy}
                          onClick={() => run(u.id, () => toggleUserStatus(u.id))}
                        >
                          {u.status === 'Disabled' ? 'Enable' : 'Disable'}
                        </Button>
                        <Button
                          size="sm"
                          variant="outline-danger"
                          disabled={busy}
                          onClick={() => {
                            if (window.confirm('Soft-delete this user?')) run(u.id, () => deleteUser(u.id));
                          }}
                        >
                          Delete
                        </Button>
                      </>
                    )}
                  </td>
                </tr>
              );
            })}
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
