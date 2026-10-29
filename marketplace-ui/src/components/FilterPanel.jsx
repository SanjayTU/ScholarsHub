import { useEffect, useState } from 'react';
import { Button, Form } from 'react-bootstrap';
import { getCategories, getDepartments, getEducationLevels } from '../api/lookupApi';

function flattenCategories(tree, acc = []) {
  (tree || []).forEach((node) => {
    if (node.children?.length) {
      node.children.forEach((child) => acc.push({ ...child, group: node.name }));
    } else {
      acc.push({ ...node, group: node.name });
    }
  });
  return acc;
}

export default function FilterPanel({ values, onApply, onClear }) {
  const [levels, setLevels] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [categories, setCategories] = useState([]);
  const [local, setLocal] = useState(values);

  useEffect(() => { setLocal(values); }, [values]);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const [lv, dp, cat] = await Promise.all([
          getEducationLevels(),
          getDepartments(),
          getCategories(),
        ]);
        if (cancelled) return;
        setLevels(lv || []);
        setDepartments(dp || []);
        setCategories(flattenCategories(cat || []));
      } catch {
        /* lookups fail silently; form still usable */
      }
    })();
    return () => { cancelled = true; };
  }, []);

  const onChange = (e) => {
    const { name, value } = e.target;
    setLocal((f) => ({ ...f, [name]: value }));
  };

  const submit = (e) => {
    e.preventDefault();
    onApply(local);
  };

  return (
    <Form onSubmit={submit} className="text-start">
      <Form.Group className="mb-3">
        <Form.Label>Category</Form.Label>
        <Form.Select name="categoryId" value={local.categoryId || ''} onChange={onChange}>
          <option value="">All</option>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>{c.group && c.group !== c.name ? `${c.group} — ${c.name}` : c.name}</option>
          ))}
        </Form.Select>
      </Form.Group>
      <Form.Group className="mb-3">
        <Form.Label>Education level</Form.Label>
        <Form.Select name="educationLevelId" value={local.educationLevelId || ''} onChange={onChange}>
          <option value="">All</option>
          {levels.map((l) => (
            <option key={l.id} value={l.id}>{l.name}</option>
          ))}
        </Form.Select>
      </Form.Group>
      <Form.Group className="mb-3">
        <Form.Label>Department</Form.Label>
        <Form.Select name="departmentId" value={local.departmentId || ''} onChange={onChange}>
          <option value="">All</option>
          {departments.map((d) => (
            <option key={d.id} value={d.id}>{d.name}</option>
          ))}
        </Form.Select>
      </Form.Group>
      <Form.Group className="mb-3">
        <Form.Label>Min price (₹)</Form.Label>
        <Form.Control type="number" min={0} name="minPrice" value={local.minPrice || ''} onChange={onChange} />
      </Form.Group>
      <Form.Group className="mb-3">
        <Form.Label>Max price (₹)</Form.Label>
        <Form.Control type="number" min={0} name="maxPrice" value={local.maxPrice || ''} onChange={onChange} />
      </Form.Group>
      <Form.Group className="mb-3">
        <Form.Label>Location</Form.Label>
        <Form.Control name="location" value={local.location || ''} onChange={onChange} />
      </Form.Group>
      <Form.Group className="mb-3">
        <Form.Label>Sort</Form.Label>
        <Form.Select name="sort" value={local.sort || 'newest'} onChange={onChange}>
          <option value="newest">Newest</option>
          <option value="price_asc">Price: low to high</option>
          <option value="price_desc">Price: high to low</option>
        </Form.Select>
      </Form.Group>
      <div className="d-grid gap-2">
        <Button type="submit">Apply filters</Button>
        <Button type="button" variant="outline-secondary" onClick={onClear}>Clear</Button>
      </div>
    </Form>
  );
}
