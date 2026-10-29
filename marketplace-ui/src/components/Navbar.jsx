import { useContext, useEffect, useState } from 'react';
import { Link, NavLink, useNavigate } from 'react-router-dom';
import { Badge, Button, Container, Nav, Navbar as BsNavbar } from 'react-bootstrap';
import { getUnreadCount } from '../api/chatApi';
import { useAuth } from '../context/AuthContext';
import { ThemeContext } from '../context/ThemeContext';

export default function AppNavbar() {
  const { isAuthenticated, user, logout } = useAuth();
  const { theme, toggleTheme } = useContext(ThemeContext);
  const navigate = useNavigate();
  const [unread, setUnread] = useState(0);

  useEffect(() => {
    if (!isAuthenticated) {
      setUnread(0);
      return undefined;
    }
    let cancelled = false;
    const load = async () => {
      try {
        const data = await getUnreadCount();
        if (!cancelled) setUnread(data?.unreadCount ?? 0);
      } catch {
        if (!cancelled) setUnread(0);
      }
    };
    load();
    const timer = setInterval(load, 15000);
    return () => {
      cancelled = true;
      clearInterval(timer);
    };
  }, [isAuthenticated]);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <BsNavbar expand="md" className="marketplace-navbar glass-nav sticky-top shadow-sm mb-4">
      <Container>
        <BsNavbar.Brand as={Link} to="/" className="d-flex align-items-center gap-2 fw-bold">
          <span className="d-inline-flex align-items-center justify-content-center rounded-3 bg-primary text-white fw-bold" style={{ width: 36, height: 36 }}>
            SM
          </span>
          <span>Student Marketplace</span>
        </BsNavbar.Brand>
        <BsNavbar.Toggle aria-controls="main-nav" />
        <BsNavbar.Collapse id="main-nav">
          <Nav className="ms-auto align-items-md-center gap-md-2">
            <Button
              type="button"
              variant="outline-secondary"
              className="rounded-pill"
              onClick={toggleTheme}
              aria-label={`Switch to ${theme === 'light' ? 'dark' : 'light'} mode`}
              title={`Switch to ${theme === 'light' ? 'dark' : 'light'} mode`}
            >
              {theme === 'light' ? '🌙' : '☀️'}
            </Button>
            <Nav.Link as={NavLink} to="/listings">
              Browse
            </Nav.Link>
            {!isAuthenticated ? (
              <>
                <Nav.Link as={NavLink} to="/login">
                  Login
                </Nav.Link>
                <Nav.Link as={NavLink} to="/register">
                  Register
                </Nav.Link>
              </>
            ) : (
              <>
                <Nav.Link as={NavLink} to="/sell">
                  Sell
                </Nav.Link>
                <Nav.Link as={NavLink} to="/my-listings">
                  My listings
                </Nav.Link>
                {user?.role === 'Admin' && (
                  <Nav.Link as={NavLink} to="/admin">
                    Admin Panel
                  </Nav.Link>
                )}
                <Nav.Link as={NavLink} to="/chats" className="d-flex align-items-center gap-1">
                  Inbox
                  {unread > 0 && <Badge bg="danger">{unread}</Badge>}
                </Nav.Link>
                <Nav.Link as={NavLink} to="/profile">
                  {user?.fullName || 'Profile'}
                </Nav.Link>
                <Button variant="outline-primary" className="rounded-pill fw-semibold px-3" size="sm" onClick={handleLogout}>
                  Logout
                </Button>
              </>
            )}
          </Nav>
        </BsNavbar.Collapse>
      </Container>
    </BsNavbar>
  );
}
