import { useState } from 'react';
import { Alert, Button, Card, Col, Container, Form, Row, Spinner } from 'react-bootstrap';
import { Link, useNavigate } from 'react-router-dom';
import api from '../../api/axiosInstance';
import { useAuth } from '../../context/AuthContext';
import { DEPARTMENTS, EDUCATION_LEVELS, PASSWORD_REGEX } from '../../utils/lookups';

function apiError(err, fallback) {
  const data = err.response?.data;
  if (data?.errors) {
    const first = Object.values(data.errors).flat()[0];
    if (first) return first;
  }
  return data?.message || fallback;
}

export default function EditProfile() {
  const { user, refreshUser } = useAuth();
  const navigate = useNavigate();

  const [form, setForm] = useState(() => ({
    fullName: user?.fullName || '',
    educationLevelId: user?.educationLevelId?.toString() || '',
    institution: user?.institution || '',
    departmentId: user?.departmentId?.toString() || '',
    semesterOrClass: user?.semesterOrClass?.toString() || '',
    locality: user?.locality || '',
    showPhone: user?.showPhone ?? true,
  }));
  const [pw, setPw] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [profileMsg, setProfileMsg] = useState({ type: '', text: '' });
  const [pwMsg, setPwMsg] = useState({ type: '', text: '' });
  const [saving, setSaving] = useState(false);
  const [savingPw, setSavingPw] = useState(false);

  const onChange = (e) => {
    const { name, value, type, checked } = e.target;
    setForm((f) => ({ ...f, [name]: type === 'checkbox' ? checked : value }));
  };

  const saveProfile = async (e) => {
    e.preventDefault();
    if (form.fullName.trim().length < 2) return setProfileMsg({ type: 'danger', text: 'Full name is required.' });
    if (!form.educationLevelId) return setProfileMsg({ type: 'danger', text: 'Select an education level.' });
    if (form.institution.trim().length < 2) return setProfileMsg({ type: 'danger', text: 'Institution is required.' });
    const sem = Number(form.semesterOrClass);
    if (!sem || sem < 1 || sem > 12) return setProfileMsg({ type: 'danger', text: 'Semester/class must be 1–12.' });
    if (form.locality.trim().length < 2) return setProfileMsg({ type: 'danger', text: 'Locality is required.' });

    setSaving(true);
    setProfileMsg({ type: '', text: '' });
    try {
      await api.put('/users/me', {
        fullName: form.fullName.trim(),
        educationLevelId: Number(form.educationLevelId),
        institution: form.institution.trim(),
        departmentId: form.departmentId ? Number(form.departmentId) : null,
        semesterOrClass: Number(form.semesterOrClass),
        locality: form.locality.trim(),
        showPhone: form.showPhone,
      });
      await refreshUser();
      setProfileMsg({ type: 'success', text: 'Profile updated.' });
      navigate('/profile');
    } catch (err) {
      setProfileMsg({ type: 'danger', text: apiError(err, 'Could not update profile.') });
    } finally {
      setSaving(false);
    }
  };

  const savePassword = async (e) => {
    e.preventDefault();
    if (!pw.currentPassword) return setPwMsg({ type: 'danger', text: 'Current password is required.' });
    if (!PASSWORD_REGEX.test(pw.newPassword)) {
      return setPwMsg({ type: 'danger', text: 'New password must be at least 8 characters with a letter and a digit.' });
    }
    if (pw.newPassword !== pw.confirmPassword) return setPwMsg({ type: 'danger', text: 'New passwords do not match.' });

    setSavingPw(true);
    setPwMsg({ type: '', text: '' });
    try {
      await api.put('/users/me/password', {
        currentPassword: pw.currentPassword,
        newPassword: pw.newPassword,
      });
      setPw({ currentPassword: '', newPassword: '', confirmPassword: '' });
      setPwMsg({ type: 'success', text: 'Password changed.' });
    } catch (err) {
      setPwMsg({ type: 'danger', text: apiError(err, 'Could not change password.') });
    } finally {
      setSavingPw(false);
    }
  };

  if (!user) return null;

  return (
    <Container className="py-3" style={{ maxWidth: 720 }}>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h4 mb-0">Edit profile</h1>
        <Button as={Link} to="/profile" variant="outline-secondary" size="sm">Back</Button>
      </div>

      <Card className="mb-4">
        <Card.Body>
          <Card.Title className="h6">Profile details</Card.Title>
          {profileMsg.text && <Alert variant={profileMsg.type}>{profileMsg.text}</Alert>}
          <Form onSubmit={saveProfile} noValidate>
            <Row>
              <Col md={12}>
                <Form.Group className="mb-3">
                  <Form.Label>Full name</Form.Label>
                  <Form.Control name="fullName" value={form.fullName} onChange={onChange} />
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
              <Col md={12}>
                <Form.Check
                  className="mb-3"
                  type="switch"
                  id="showPhone"
                  name="showPhone"
                  label="Allow Call Seller (show phone to logged-in users)"
                  checked={form.showPhone}
                  onChange={onChange}
                />
              </Col>
            </Row>
            <Button type="submit" disabled={saving}>
              {saving ? <Spinner size="sm" animation="border" /> : 'Save profile'}
            </Button>
          </Form>
        </Card.Body>
      </Card>

      <Card>
        <Card.Body>
          <Card.Title className="h6">Change password</Card.Title>
          {pwMsg.text && <Alert variant={pwMsg.type}>{pwMsg.text}</Alert>}
          <Form onSubmit={savePassword} noValidate>
            <Form.Group className="mb-3">
              <Form.Label>Current password</Form.Label>
              <Form.Control type="password" value={pw.currentPassword} onChange={(e) => setPw((p) => ({ ...p, currentPassword: e.target.value }))} />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>New password</Form.Label>
              <Form.Control type="password" value={pw.newPassword} onChange={(e) => setPw((p) => ({ ...p, newPassword: e.target.value }))} />
            </Form.Group>
            <Form.Group className="mb-3">
              <Form.Label>Confirm new password</Form.Label>
              <Form.Control type="password" value={pw.confirmPassword} onChange={(e) => setPw((p) => ({ ...p, confirmPassword: e.target.value }))} />
            </Form.Group>
            <Button type="submit" variant="outline-primary" disabled={savingPw}>
              {savingPw ? <Spinner size="sm" animation="border" /> : 'Change password'}
            </Button>
          </Form>
        </Card.Body>
      </Card>
    </Container>
  );
}
