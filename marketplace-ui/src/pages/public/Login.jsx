import { useState } from 'react';
import { Alert, Button, Card, Container, Form, Spinner } from 'react-bootstrap';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

function apiError(err, fallback) {
  return err.response?.data?.message || fallback;
}

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from?.pathname || '/profile';

  const [form, setForm] = useState({ emailOrMobile: '', password: '' });
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const onChange = (e) => setForm((f) => ({ ...f, [e.target.name]: e.target.value }));

  const validate = () => {
    if (!form.emailOrMobile.trim()) return 'Email or mobile is required.';
    if (!form.password) return 'Password is required.';
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
      await login(form.emailOrMobile.trim(), form.password);
      navigate(from, { replace: true });
    } catch (err) {
      setError(apiError(err, 'Invalid email/mobile or password.'));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Container className="py-3" style={{ maxWidth: 480 }}>
      <Card>
        <Card.Body>
          <Card.Title className="mb-3">Login</Card.Title>
          {error && <Alert variant="danger">{error}</Alert>}
          <Form onSubmit={onSubmit} noValidate>
            <Form.Group className="mb-3">
              <Form.Label>Email or mobile</Form.Label>
              <Form.Control
                name="emailOrMobile"
                value={form.emailOrMobile}
                onChange={onChange}
                placeholder="you@college.edu or 9876543210"
                autoComplete="username"
              />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Password</Form.Label>
              <Form.Control
                type="password"
                name="password"
                value={form.password}
                onChange={onChange}
                autoComplete="current-password"
              />
            </Form.Group>
            <Button type="submit" className="w-100" disabled={submitting}>
              {submitting ? <Spinner size="sm" animation="border" /> : 'Login'}
            </Button>
          </Form>
          <p className="mt-3 mb-0 text-center">
            New here? <Link to="/register">Register</Link>
          </p>
        </Card.Body>
      </Card>
    </Container>
  );
}
