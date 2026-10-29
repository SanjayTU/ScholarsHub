import { Badge } from 'react-bootstrap';

const VARIANTS = {
  Available: 'success',
  Sold: 'secondary',
  Unavailable: 'warning',
  Removed: 'danger',
};

export default function StatusBadge({ status }) {
  const key = status || '';
  return (
    <Badge bg={VARIANTS[key] || 'light'} text={VARIANTS[key] ? undefined : 'dark'}>
      {key || 'Unknown'}
    </Badge>
  );
}
