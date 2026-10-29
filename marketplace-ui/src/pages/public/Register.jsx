import { useState } from 'react';
import { Alert, Button, Card, Container, Form, Row, Col, Spinner } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { DEPARTMENTS, EDUCATION_LEVELS, MOBILE_REGEX, PASSWORD_REGEX } from '../../utils/lookups';

function apiError(err, fallback) {
  const data = err.response?.data;
  if (data?.errors) {
    const first = Object.values(data.errors).flat()[0];
    if (first) return first;
  }
  return data?.message || fallback;
}

const empty = {
  fullName: '',
  mobile: '',
  email: '',
  password: '',
  educationLevelId: '',
  institution: '',
  departmentId: '',
  semesterOrClass: '',
  locality: '',
};

export default function Register() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState(empty);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const onChange = (e) => setForm((f) => ({ ...f, [e.target.name]: e.target.value }));

  const validate = () => {
    if (form.fullName.trim().length < 2) return 'Full name is required.';
    if (!MOBILE_REGEX.test(form.mobile.trim())) return 'Mobile must be a 10-digit Indian number starting with 6–9.';
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email.trim())) return 'Enter a valid email.';
    if (!PASSWORD_REGEX.test(form.password)) return 'Password must be at least 8 characters with a letter and a digit.';
    if (!form.educationLevelId) return 'Select an education level.';
    if (form.institution.trim().length < 2) return 'Institution is required.';
    const sem = Number(form.semesterOrClass);
    if (!sem || sem < 1 || sem > 12) return 'Semester/class must be between 1 and 12.';
    if (form.locality.trim().length < 2) return 'Locality is required.';
    return '';
  };

  const onSubmit = async (e) => {
    e.preventDefault();
    const msg = validate();
    if (msg) {
      setError(msg);
      return;
    }
    setError('');
    setSubmitting(true);
    try {
      await register({
        fullName: form.fullName.trim(),
        mobile: form.mobile.trim(),
        email: form.email.trim(),
        password: form.password,
        educationLevelId: Number(form.educationLevelId),
        institution: form.institution.trim(),
        departmentId: form.departmentId ? Number(form.departmentId) : null,
        semesterOrClass: Number(form.semesterOrClass),
        locality: form.locality.trim(),
      });
      navigate('/profile', { replace: true });
    } catch (err) {
      setError(apiError(err, 'Registration failed.'));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Container className="py-3" style={{ maxWidth: 720 }}>
      <Card>
        <Card.Body>
          <Card.Title className="mb-3">Create account</Card.Title>
          {error && <Alert variant="danger">{error}</Alert>}
          <Form onSubmit={onSubmit} noValidate>
            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Full name</Form.Label>
                  <Form.Control name="fullName" value={form.fullName} onChange={onChange} />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Mobile</Form.Label>
                  <Form.Control name="mobile" value={form.mobile} onChange={onChange} placeholder="9876543210" />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Email</Form.Label>
                  <Form.Control type="email" name="email" value={form.email} onChange={onChange} />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Password</Form.Label>
                  <Form.Control type="password" name="password" value={form.password} onChange={onChange} />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Education level</Form.Label>
                  <Form.Select name="educationLevelId" value={form.educationLevelId} onChange={onChange}>
                    <option value="">Select</option>
                    {EDUCATION_LEVELS.map((l) => (
                      <option key={l.id} value={l.id}>{l.name}</option>
                    ))}
                  </Form.Select>
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Department (optional)</Form.Label>
                  <Form.Select name="departmentId" value={form.departmentId} onChange={onChange}>
                    <option value="">None</option>
                    {DEPARTMENTS.map((d) => (
                      <option key={d.id} value={d.id}>{d.name}</option>
                    ))}
                  </Form.Select>
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Institution / College</Form.Label>
                  <Form.Control name="institution" value={form.institution} onChange={onChange} />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Semester / Class</Form.Label>
                  <Form.Control type="number" min={1} max={12} name="semesterOrClass" value={form.semesterOrClass} onChange={onChange} />
                </Form.Group>
              </Col>
              <Col md={12}>
                <Form.Group className="mb-3">
                  <Form.Label>Locality</Form.Label>
                  <Form.Control name="locality" value={form.locality} onChange={onChange} />
                </Form.Group>
              </Col>
            </Row>
            <Button type="submit" className="w-100" disabled={submitting}>
              {submitting ? <Spinner size="sm" animation="border" /> : 'Register'}
            </Button>
          </Form>
          <p className="mt-3 mb-0 text-center">
            Already have an account? <Link to="/login">Login</Link>
          </p>
        </Card.Body>
      </Card>
    </Container>
  );
}
