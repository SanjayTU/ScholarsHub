import { useCallback, useEffect, useState } from 'react';
import { Alert, Button, Container, Form, Modal, Nav, Spinner, Table } from 'react-bootstrap';
import { NavLink } from 'react-router-dom';
import { addCategory, getAdminCategories, toggleCategory, updateCategory } from '../../api/adminApi';

function apiError(err, fallback) {
  const data = err.response?.data;
  if (data?.errors) {
    const first = Object.values(data.errors).flat()[0];
    if (first) return first;
  }
  return data?.message || fallback;
}

const emptyForm = { name: '', parentId: '' };

export default function Categories() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [busyId, setBusyId] = useState(null);
  const [show, setShow] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await getAdminCategories();
      setItems(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(apiError(err, 'Could not load categories.'));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const openAdd = () => {
    setEditing(null);
    setForm(emptyForm);
    setShow(true);
  };

  const openEdit = (c) => {
    setEditing(c);
    setForm({ name: c.name, parentId: c.parentId ?? '' });
    setShow(true);
  };

  const save = async (e) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    const payload = {
      name: form.name.trim(),
      parentId: form.parentId === '' ? null : Number(form.parentId),
    };
    try {
      if (editing) await updateCategory(editing.id, payload);
      else await addCategory(payload);
      setShow(false);
      await load();
    } catch (err) {
      setError(apiError(err, 'Could not save category.'));
    } finally {
      setSaving(false);
    }
  };

  const onToggle = async (id) => {
    setBusyId(id);
    setError('');
    try {
      await toggleCategory(id);
      await load();
    } catch (err) {
      setError(apiError(err, 'Could not update category.'));
    } finally {
      setBusyId(null);
    }
  };

  return (
    <Container className="py-3">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">Categories</h1>
        <Button size="sm" onClick={openAdd}>Add category</Button>
      </div>
      <Nav variant="pills" className="mb-3 flex-wrap">
        <Nav.Link as={NavLink} to="/admin" end>Dashboard</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/users">Users</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/listings">Listings</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/categories">Categories</Nav.Link>
        <Nav.Link as={NavLink} to="/admin/reports">Reports</Nav.Link>
      </Nav>
      {error && <Alert variant="danger">{error}</Alert>}
      {loading ? (
        <div className="d-flex justify-content-center py-4"><Spinner animation="border" /></div>
      ) : (
        <Table responsive hover size="sm" className="align-middle">
          <thead>
            <tr>
              <th>Name</th>
              <th>Parent</th>
              <th>Listings</th>
              <th>Status</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {items.length === 0 && (
              <tr><td colSpan={5} className="text-muted">No categories.</td></tr>
            )}
            {items.map((c) => (
              <tr key={c.id}>
                <td>{c.name}</td>
                <td>{c.parentName || '—'}</td>
                <td>{c.listingCount}</td>
                <td>{c.isActive ? 'Active' : 'Inactive'}</td>
                <td className="text-nowrap">
                  <Button size="sm" variant="outline-primary" className="me-1" onClick={() => openEdit(c)}>Edit</Button>
                  <Button
                    size="sm"
                    variant={c.isActive ? 'outline-warning' : 'outline-success'}
                    disabled={busyId === c.id}
                    onClick={() => onToggle(c.id)}
                  >
                    {c.isActive ? 'Deactivate' : 'Activate'}
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}

      <Modal show={show} onHide={() => setShow(false)}>
        <Form onSubmit={save}>
          <Modal.Header closeButton>
            <Modal.Title>{editing ? 'Edit category' : 'Add category'}</Modal.Title>
          </Modal.Header>
          <Modal.Body>
            <Form.Group className="mb-3">
              <Form.Label>Name</Form.Label>
              <Form.Control
                required
                minLength={2}
                maxLength={100}
                value={form.name}
                onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
              />
            </Form.Group>
            <Form.Group>
              <Form.Label>Parent</Form.Label>
              <Form.Select
                value={form.parentId}
                onChange={(e) => setForm((f) => ({ ...f, parentId: e.target.value }))}
              >
                <option value="">None (top-level)</option>
                {items
                  .filter((c) => !editing || c.id !== editing.id)
                  .map((c) => (
                    <option key={c.id} value={c.id}>{c.name}</option>
                  ))}
              </Form.Select>
            </Form.Group>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="secondary" onClick={() => setShow(false)}>Cancel</Button>
            <Button type="submit" disabled={saving}>{saving ? 'Saving…' : 'Save'}</Button>
          </Modal.Footer>
        </Form>
      </Modal>
    </Container>
  );
}
