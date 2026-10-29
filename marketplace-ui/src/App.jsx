import { Route, Routes } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import AppNavbar from './components/Navbar';
import AdminRoute from './components/AdminRoute';
import ProtectedRoute, { GuestRoute } from './components/ProtectedRoute';
import Home from './pages/public/Home';
import Login from './pages/public/Login';
import Register from './pages/public/Register';
import Listings from './pages/public/Listings';
import ListingDetails from './pages/public/ListingDetails';
import MyProfile from './pages/student/MyProfile';
import EditProfile from './pages/student/EditProfile';
import CreateListing from './pages/student/CreateListing';
import EditListing from './pages/student/EditListing';
import MyListings from './pages/student/MyListings';
import Inbox from './pages/student/Inbox';
import Conversation from './pages/student/Conversation';
import Dashboard from './pages/admin/Dashboard';
import Users from './pages/admin/Users';
import AdminListings from './pages/admin/Listings';
import Categories from './pages/admin/Categories';
import Reports from './pages/admin/Reports';

export default function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <AppNavbar />
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/listings" element={<Listings />} />
          <Route path="/listings/:id" element={<ListingDetails />} />
          <Route path="/login" element={<GuestRoute><Login /></GuestRoute>} />
          <Route path="/register" element={<GuestRoute><Register /></GuestRoute>} />
          <Route path="/profile" element={<ProtectedRoute><MyProfile /></ProtectedRoute>} />
          <Route path="/profile/edit" element={<ProtectedRoute><EditProfile /></ProtectedRoute>} />
          <Route path="/sell" element={<ProtectedRoute><CreateListing /></ProtectedRoute>} />
          <Route path="/my-listings" element={<ProtectedRoute><MyListings /></ProtectedRoute>} />
          <Route path="/listings/:id/edit" element={<ProtectedRoute><EditListing /></ProtectedRoute>} />
          <Route path="/chats" element={<ProtectedRoute><Inbox /></ProtectedRoute>} />
          <Route path="/chats/:id" element={<ProtectedRoute><Conversation /></ProtectedRoute>} />
          <Route element={<AdminRoute />}>
            <Route path="/admin" element={<Dashboard />} />
            <Route path="/admin/users" element={<Users />} />
            <Route path="/admin/listings" element={<AdminListings />} />
            <Route path="/admin/categories" element={<Categories />} />
            <Route path="/admin/reports" element={<Reports />} />
          </Route>
        </Routes>
      </AuthProvider>
    </ThemeProvider>
  );
}
