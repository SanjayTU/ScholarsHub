import { useEffect, useMemo, useState } from 'react';
import { Alert, Button, Col, Container, Form, Pagination, Row, Spinner } from 'react-bootstrap';
import { useSearchParams } from 'react-router-dom';
import { searchListings } from '../../api/publicApi';
import FilterPanel from '../../components/FilterPanel';
import ListingCard from '../../components/ListingCard';

function apiError(err, fallback) {
  const data = err.response?.data;
  if (data?.errors) {
    const first = Object.values(data.errors).flat()[0];
    if (first) return first;
  }
  return data?.message || fallback;
}

const FILTER_KEYS = ['q', 'categoryId', 'educationLevelId', 'departmentId', 'minPrice', 'maxPrice', 'location', 'sort', 'page'];

function readFilters(searchParams) {
  const values = {};
  FILTER_KEYS.forEach((key) => {
    const v = searchParams.get(key);
    if (v) values[key] = v;
  });
  if (!values.sort) values.sort = 'newest';
  if (!values.page) values.page = '1';
  return values;
}

function toSearchParams(values) {
  const next = new URLSearchParams();
  FILTER_KEYS.forEach((key) => {
    const v = values[key];
    if (v && !(key === 'sort' && v === 'newest') && !(key === 'page' && v === '1')) {
      next.set(key, String(v));
    }
  });
  return next;
}

export default function Listings() {
  const [searchParams, setSearchParams] = useSearchParams();
  const filters = useMemo(() => readFilters(searchParams), [searchParams]);
  const [qInput, setQInput] = useState(filters.q || '');
  const [data, setData] = useState({ items: [], totalCount: 0, totalPages: 0, page: 1, pageSize: 12 });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => { setQInput(filters.q || ''); }, [filters.q]);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError('');
      try {
        const result = await searchListings({
          q: filters.q,
          categoryId: filters.categoryId,
          educationLevelId: filters.educationLevelId,
          departmentId: filters.departmentId,
          minPrice: filters.minPrice,
          maxPrice: filters.maxPrice,
          location: filters.location,
          sort: filters.sort,
          page: filters.page || 1,
          pageSize: 12,
        });
        if (!cancelled) {
          setData({
            items: result.items || [],
            totalCount: result.totalCount ?? 0,
            totalPages: result.totalPages ?? 0,
            page: result.page ?? 1,
            pageSize: result.pageSize ?? 12,
          });
        }
      } catch (err) {
        if (!cancelled) setError(apiError(err, 'Could not load listings.'));
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [searchParams]);

  const applyFilters = (next) => {
    setSearchParams(toSearchParams({ ...next, q: qInput, page: '1' }));
  };

  const searchKeyword = (e) => {
    e.preventDefault();
    setSearchParams(toSearchParams({ ...filters, q: qInput, page: '1' }));
  };

  const goPage = (page) => {
    setSearchParams(toSearchParams({ ...filters, page: String(page) }));
  };

  const pages = [];
  const totalPages = data.totalPages || 0;
  const current = Number(filters.page) || 1;
  const start = Math.max(1, current - 2);
  const end = Math.min(totalPages, start + 4);
  for (let p = start; p <= end; p += 1) pages.push(p);

  return (
    <Container className="py-3 py-md-4">
      <div className="mb-4">
        <p className="text-primary text-uppercase fw-semibold small mb-1">Marketplace</p>
        <h1 className="h2 fw-bold mb-1">Browse listings</h1>
        <p className="text-muted mb-0">Find useful items from students in your community.</p>
      </div>
      <Form onSubmit={searchKeyword} className="d-flex flex-column flex-sm-row gap-2 mb-4">
        <Form.Control className="rounded-3 py-2" value={qInput} onChange={(e) => setQInput(e.target.value)} placeholder="Search title or description" />
        <Button type="submit" className="rounded-3 px-4">Search</Button>
      </Form>
      {error && <Alert variant="danger">{error}</Alert>}
      <Row>
        <Col lg={3} className="mb-4">
          <div className="bg-white p-3 p-lg-4 rounded-4 shadow-sm">
            <h2 className="h6 fw-bold mb-3">Filters</h2>
            <FilterPanel
              values={filters}
              onApply={applyFilters}
              onClear={() => { setQInput(''); setSearchParams(new URLSearchParams()); }}
            />
          </div>
        </Col>
        <Col lg={9}>
          {loading ? (
            <div className="d-flex justify-content-center py-5">
              <Spinner animation="border" />
            </div>
          ) : (
            <>
              <p className="text-muted text-start mb-3">{data.totalCount} result{data.totalCount === 1 ? '' : 's'}</p>
              {data.items.length === 0 && <Alert variant="light" className="border rounded-4 p-4">No listings match these filters.</Alert>}
              <Row className="row-cols-1 row-cols-md-2 row-cols-xl-3 g-4">
                {data.items.map((item) => (
                  <Col key={item.id}>
                    <ListingCard listing={item} />
                  </Col>
                ))}
              </Row>
              {totalPages > 1 && (
                <Pagination className="mt-4 justify-content-center">
                  <Pagination.Prev disabled={current <= 1} onClick={() => goPage(current - 1)} />
                  {pages.map((p) => (
                    <Pagination.Item key={p} active={p === current} onClick={() => goPage(p)}>{p}</Pagination.Item>
                  ))}
                  <Pagination.Next disabled={current >= totalPages} onClick={() => goPage(current + 1)} />
                </Pagination>
              )}
            </>
          )}
        </Col>
      </Row>
    </Container>
  );
}
