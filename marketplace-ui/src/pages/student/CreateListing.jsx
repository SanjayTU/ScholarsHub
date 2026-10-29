import { useEffect, useMemo, useState } from 'react';
import { Alert, Button, Card, Col, Container, Form, Row, Spinner } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { getCategories, getConditions, getDepartments, getEducationLevels } from '../../api/lookupApi';
import { createListing, listingImageUrl } from '../../api/listingApi';
import { useAuth } from '../../context/AuthContext';

const MAX_IMAGES = 5;
const MAX_BYTES = 2 * 1024 * 1024;
const ALLOWED = ['image/jpeg', 'image/png', 'image/webp'];

function apiError(err, fallback) {
  const data = err.response?.data;
  if (data?.errors) {
    const first = Object.values(data.errors).flat()[0];
    if (first) return first;
  }
  return data?.message || fallback;
}

function isCollegeLevel(name) {
  return /undergraduate|postgraduate/i.test(name || '');
}

function departmentOptionalOnly(name) {
  return /^(Primary|High School)$/i.test((name || '').trim());
}

function flattenCategories(tree, acc = []) {
  (tree || []).forEach((node) => {
    if (node.children?.length) {
      node.children.forEach((child) => acc.push({ ...child, group: node.name }));
    } else if (node.parentId == null) {
      acc.push({ ...node, group: node.name });
    }
  });
  return acc;
}

export function ListingForm({ mode, initial, existingImageUrls, onSubmit, submitting, error }) {
  const [levels, setLevels] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [categories, setCategories] = useState([]);
  const [conditions, setConditions] = useState([]);
  const [loadError, setLoadError] = useState('');
  const [loadingLookups, setLoadingLookups] = useState(true);
  const [form, setForm] = useState(() => ({
    title: initial?.title || '',
    categoryId: initial?.categoryId?.toString() || '',
    educationLevelId: initial?.educationLevelId?.toString() || '',
    departmentId: initial?.departmentId?.toString() || '',
    semesterOrClass: initial?.semesterOrClass?.toString() || '',
    condition: initial?.conditionId?.toString() || initial?.condition?.toString() || '',
    price: initial?.price?.toString() ?? '',
    location: initial?.location || '',
    description: initial?.description || '',
  }));
  const [images, setImages] = useState([]);
  const [localError, setLocalError] = useState('');

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const [lv, dp, cat, cond] = await Promise.all([
          getEducationLevels(),
          getDepartments(),
          getCategories(),
          getConditions(),
        ]);
        if (cancelled) return;
        setLevels(lv || []);
        setDepartments(dp || []);
        setCategories(cat || []);
        setConditions(cond || []);
        if (initial?.condition && cond?.length) {
          const match = cond.find((c) => c.name === initial.condition || String(c.id) === String(initial.condition));
          if (match) {
            setForm((f) => ({ ...f, condition: String(match.id) }));
          }
        }
      } catch (err) {
        if (!cancelled) setLoadError(apiError(err, 'Could not load form options.'));
      } finally {
        if (!cancelled) setLoadingLookups(false);
      }
    })();
    return () => { cancelled = true; };
  }, [initial?.condition]);

  const selectedLevel = levels.find((l) => String(l.id) === String(form.educationLevelId));
  const college = isCollegeLevel(selectedLevel?.name);
  const hideDepartment = departmentOptionalOnly(selectedLevel?.name);
  const semMax = college ? 8 : 12;
  const semLabel = college ? 'Semester' : 'Class';
  const leafCategories = useMemo(() => flattenCategories(categories), [categories]);

  const onChange = (e) => {
    const { name, value } = e.target;
    setForm((f) => {
      const next = { ...f, [name]: value };
      if (name === 'educationLevelId') {
        next.semesterOrClass = '';
        if (departmentOptionalOnly(levels.find((l) => String(l.id) === value)?.name)) {
          next.departmentId = '';
        }
      }
      return next;
    });
  };

  const onFiles = (e) => {
    const files = Array.from(e.target.files || []);
    e.target.value = '';
    const invalid = files.find((f) => !ALLOWED.includes(f.type) || f.size > MAX_BYTES);
    if (invalid) {
      setLocalError('Images must be jpg, png, or webp and at most 2 MB each.');
      return;
    }
    setImages((prev) => {
      const merged = [...prev, ...files].slice(0, MAX_IMAGES);
      return merged;
    });
    setLocalError('');
  };

  const removeImage = (idx) => setImages((prev) => prev.filter((_, i) => i !== idx));

  const validate = () => {
    if (form.title.trim().length < 5 || form.title.trim().length > 120) return 'Title must be 5–120 characters.';
    if (!form.categoryId) return 'Select a category.';
    if (!form.educationLevelId) return 'Select an education level.';
    if (!form.condition) return 'Select a condition.';
    const price = Number(form.price);
    if (Number.isNaN(price) || price < 0 || price > 100000) return 'Price must be between ₹0 and ₹100000.';
    if (form.location.trim().length < 2) return 'Location is required.';
    if (form.description.trim().length === 0 || form.description.length > 2000) return 'Description is required (max 2000 characters).';
    if (form.semesterOrClass) {
      const n = Number(form.semesterOrClass);
      if (n < 1 || n > semMax) return `${semLabel} must be 1–${semMax}.`;
    }
    if (mode === 'create' && images.length < 1) return 'Upload at least 1 image (max 5).';
    if (images.length > MAX_IMAGES) return 'A listing can have at most 5 images.';
    return '';
  };

  const submit = async (e) => {
    e.preventDefault();
    const msg = validate();
    if (msg) {
      setLocalError(msg);
      return;
    }
    setLocalError('');
    await onSubmit({
      title: form.title,
      categoryId: Number(form.categoryId),
      educationLevelId: Number(form.educationLevelId),
      departmentId: hideDepartment ? null : (form.departmentId ? Number(form.departmentId) : null),
      semesterOrClass: form.semesterOrClass ? Number(form.semesterOrClass) : null,
      condition: Number(form.condition),
      price: Number(form.price),
      location: form.location,
      description: form.description,
      images,
    });
  };

  if (loadingLookups) {
    return (
      <div className="d-flex justify-content-center py-5">
        <Spinner animation="border" />
      </div>
    );
  }

  return (
    <>
      {(loadError || localError || error) && (
        <Alert variant="danger">{loadError || localError || error}</Alert>
      )}
      <Form onSubmit={submit} noValidate>
        <Row>
          <Col md={12}>
            <Form.Group className="mb-3">
              <Form.Label>Title</Form.Label>
              <Form.Control name="title" value={form.title} onChange={onChange} maxLength={120} />
            </Form.Group>
          </Col>
          <Col md={6}>
            <Form.Group className="mb-3">
              <Form.Label>Category</Form.Label>
              <Form.Select name="categoryId" value={form.categoryId} onChange={onChange}>
                <option value="">Select</option>
                {leafCategories.map((c) => (
                  <option key={c.id} value={c.id}>{c.group ? `${c.group} — ${c.name}` : c.name}</option>
                ))}
              </Form.Select>
            </Form.Group>
          </Col>
          <Col md={6}>
            <Form.Group className="mb-3">
              <Form.Label>Condition</Form.Label>
              <Form.Select name="condition" value={form.condition} onChange={onChange}>
                <option value="">Select</option>
                {conditions.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </Form.Select>
            </Form.Group>
          </Col>
          <Col md={6}>
            <Form.Group className="mb-3">
              <Form.Label>Education level</Form.Label>
              <Form.Select name="educationLevelId" value={form.educationLevelId} onChange={onChange}>
                <option value="">Select</option>
                {levels.map((l) => (
                  <option key={l.id} value={l.id}>{l.name}</option>
                ))}
              </Form.Select>
            </Form.Group>
          </Col>
          {!hideDepartment && (
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>Department (optional)</Form.Label>
                <Form.Select name="departmentId" value={form.departmentId} onChange={onChange}>
                  <option value="">None</option>
                  {departments.map((d) => (
                    <option key={d.id} value={d.id}>{d.name}</option>
                  ))}
                </Form.Select>
              </Form.Group>
            </Col>
          )}
          <Col md={6}>
            <Form.Group className="mb-3">
              <Form.Label>{semLabel}</Form.Label>
              <Form.Select name="semesterOrClass" value={form.semesterOrClass} onChange={onChange}>
                <option value="">None</option>
                {Array.from({ length: semMax }, (_, i) => i + 1).map((n) => (
                  <option key={n} value={n}>{college ? `Semester ${n}` : `Class ${n}`}</option>
                ))}
              </Form.Select>
            </Form.Group>
          </Col>
          <Col md={6}>
            <Form.Group className="mb-3">
              <Form.Label>Price (₹)</Form.Label>
              <Form.Control type="number" min={0} max={100000} step="0.01" name="price" value={form.price} onChange={onChange} />
            </Form.Group>
          </Col>
          <Col md={12}>
            <Form.Group className="mb-3">
              <Form.Label>Location</Form.Label>
              <Form.Control name="location" value={form.location} onChange={onChange} />
            </Form.Group>
          </Col>
          <Col md={12}>
            <Form.Group className="mb-3">
              <Form.Label>Description</Form.Label>
              <Form.Control as="textarea" rows={4} maxLength={2000} name="description" value={form.description} onChange={onChange} />
            </Form.Group>
          </Col>
          <Col md={12}>
            <Form.Group className="mb-3">
              <Form.Label>
                Images {mode === 'create' ? '(1–5, first is cover)' : '(optional — new files replace existing)'}
              </Form.Label>
              {mode === 'edit' && existingImageUrls?.length > 0 && images.length === 0 && (
                <div className="d-flex flex-wrap gap-2 mb-2">
                  {existingImageUrls.map((src) => (
                    <img key={src} src={listingImageUrl(src)} alt="" style={{ width: 72, height: 72, objectFit: 'cover' }} className="rounded border" />
                  ))}
                </div>
              )}
              <Form.Control type="file" accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" multiple onChange={onFiles} />
              <Form.Text>jpg / png / webp, max 2 MB each, up to {MAX_IMAGES}.</Form.Text>
              {images.length > 0 && (
                <div className="d-flex flex-wrap gap-2 mt-2">
                  {images.map((file, i) => (
                    <div key={`${file.name}-${i}`} className="position-relative">
                      <img src={URL.createObjectURL(file)} alt="" style={{ width: 72, height: 72, objectFit: 'cover' }} className="rounded border" />
                      {i === 0 && <span className="badge bg-primary position-absolute top-0 start-0">Cover</span>}
                      <Button size="sm" variant="danger" className="position-absolute top-0 end-0 p-0 px-1" type="button" onClick={() => removeImage(i)}>×</Button>
                    </div>
                  ))}
                </div>
              )}
            </Form.Group>
          </Col>
        </Row>
        <Button type="submit" disabled={submitting}>
          {submitting ? <Spinner size="sm" animation="border" /> : mode === 'create' ? 'Publish listing' : 'Save changes'}
        </Button>
      </Form>
    </>
  );
}

export default function CreateListing() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (payload) => {
    setSubmitting(true);
    setError('');
    try {
      await createListing(payload);
      navigate('/my-listings');
    } catch (err) {
      setError(apiError(err, 'Could not create listing.'));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Container className="py-3" style={{ maxWidth: 800 }}>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">Sell an item</h1>
        <Button as={Link} to="/my-listings" variant="outline-secondary" size="sm">My listings</Button>
      </div>
      <Card>
        <Card.Body>
          <ListingForm
            mode="create"
            initial={{ location: user?.locality || '', educationLevelId: user?.educationLevelId, departmentId: user?.departmentId, semesterOrClass: user?.semesterOrClass }}
            onSubmit={handleSubmit}
            submitting={submitting}
            error={error}
          />
        </Card.Body>
      </Card>
    </Container>
  );
}
