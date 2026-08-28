import React, { useEffect, useState } from 'react'
import { Routes, Route, NavLink, Outlet } from 'react-router-dom'
import { api, fmtDateTime, STATUS, STATUS_CSS, PRIORITY, PRIORITY_CSS, CHANNELS } from './api'

function Badge({ text, css }) { return <span className={`badge ${css || 'secondary'}`}>{text}</span> }
function Flash({ msg }) { return msg ? <div className={`flash ${msg.ok ? 'ok' : 'err'}`}>{msg.text}</div> : null }
function Modal({ title, onClose, wide, children }) {
  return (
    <div className="modal-bg" onClick={onClose}>
      <div className="modal" style={wide ? { maxWidth: 720 } : undefined} onClick={e => e.stopPropagation()}>
        <div className="row" style={{ marginBottom: 12 }}><h2 style={{ flex: 1, margin: 0 }}>{title}</h2>
          <button className="btn gray sm" style={{ flex: 'none' }} onClick={onClose}>Đóng</button></div>{children}
      </div>
    </div>
  )
}
function Field({ label, children }) { return <div style={{ flex: 1 }}><label>{label}</label>{children}</div> }

function Layout() {
  return (
    <>
      <nav className="nav"><span className="brand">🎧 MiniCSKH</span>
        <NavLink to="/" end>Tổng quan</NavLink><NavLink to="/tickets">Ticket</NavLink>
        <NavLink to="/calls">Cuộc gọi</NavLink><NavLink to="/kb">Kiến thức</NavLink></nav>
      <div className="wrap"><Outlet /></div>
    </>
  )
}

function Dashboard() {
  const [d, setD] = useState(null); const [cache, setCache] = useState('')
  useEffect(() => { api.dashboard().then(r => { setD(r.data); setCache(r.cache) }) }, [])
  if (!d) return <p className="muted">Đang tải…</p>
  const maxA = Math.max(1, ...d.byAgent.map(x => x.open))
  return (
    <>
      <h1>Tổng quan CSKH {cache && <span className="pill">cache: {cache}</span>}</h1>
      <div className="grid kpis" style={{ marginBottom: 18 }}>
        <div className="kpi"><div className="v">{d.open}</div><div className="l">Ticket mở</div></div>
        <div className="kpi"><div className="v" style={{ color: 'var(--danger)' }}>{d.overdue}</div><div className="l">Quá hạn SLA</div></div>
        <div className="kpi"><div className="v" style={{ color: 'var(--warning)' }}>{d.unassignedOpen}</div><div className="l">Chưa gán</div></div>
        <div className="kpi"><div className="v" style={{ color: 'var(--success)' }}>{d.resolvedToday}</div><div className="l">Giải quyết hôm nay</div></div>
      </div>
      <div className="grid" style={{ gridTemplateColumns: '1fr 1fr' }}>
        <div className="card"><h2>Theo trạng thái</h2><table><tbody>{d.byStatus.map((s, i) => <tr key={i}><td><Badge text={STATUS[s.status]} css={STATUS_CSS[s.status]} /></td><td className="right">{s.count}</td></tr>)}</tbody></table></div>
        <div className="card funnel"><h2>Tải theo agent (đang mở)</h2>{d.byAgent.map((a, i) => (<div className="bar" key={i}><div className="lbl">{a.agent}</div><div className="track"><div className="fill" style={{ width: `${(a.open / maxA) * 100}%` }} /></div><div className="n">{a.open}</div></div>))}</div>
      </div>
    </>
  )
}

function Tickets() {
  const [rows, setRows] = useState([]); const [status, setStatus] = useState(''); const [q, setQ] = useState('')
  const [open, setOpen] = useState(null); const [show, setShow] = useState(false)
  const load = () => api.tickets(status === '' ? null : Number(status), null, null, q).then(r => setRows(r.data))
  useEffect(() => { load() }, [status])
  return (
    <>
      <div className="toolbar"><h1 style={{ margin: 0, flex: 'none' }}>Ticket</h1><div className="sp" />
        <select style={{ maxWidth: 150 }} value={status} onChange={e => setStatus(e.target.value)}><option value="">— Trạng thái —</option>{STATUS.map((s, i) => <option key={i} value={i}>{s}</option>)}</select>
        <input style={{ maxWidth: 180 }} placeholder="Tìm…" value={q} onChange={e => setQ(e.target.value)} onKeyDown={e => e.key === 'Enter' && load()} />
        <button className="btn ghost sm" style={{ flex: 'none' }} onClick={load}>Tìm</button>
        <button className="btn sm" style={{ flex: 'none' }} onClick={() => setShow(true)}>+ Tạo ticket</button></div>
      <div className="card" style={{ padding: 0, overflow: 'auto' }}>
        <table><thead><tr><th>Mã</th><th>Tiêu đề</th><th>Khách</th><th>Kênh</th><th>Ưu tiên</th><th>Agent</th><th>Trạng thái</th></tr></thead>
          <tbody>{rows.map(t => (
            <tr key={t.id} style={{ cursor: 'pointer' }} onClick={() => setOpen(t.id)}>
              <td>{t.code}</td><td>{t.subject}{t.overdue && <span className="badge danger" style={{ marginLeft: 4 }}>SLA</span>}</td><td>{t.customerName}</td><td>{t.channel}</td>
              <td><Badge text={PRIORITY[t.priority]} css={PRIORITY_CSS[t.priority]} /></td><td>{t.agent || '—'}</td><td><Badge text={STATUS[t.status]} css={STATUS_CSS[t.status]} /></td></tr>))}
            {rows.length === 0 && <tr><td colSpan={7} className="muted" style={{ padding: 20 }}>Không có ticket.</td></tr>}</tbody></table>
      </div>
      {open && <TicketDetail id={open} onClose={() => setOpen(null)} onChanged={load} />}
      {show && <TicketForm onClose={() => setShow(false)} onSaved={() => { setShow(false); load() }} />}
    </>
  )
}

function TicketDetail({ id, onClose, onChanged }) {
  const [t, setT] = useState(null); const [agents, setAgents] = useState([]); const [msg, setMsg] = useState(null); const [comment, setComment] = useState(''); const [internal, setInternal] = useState(false)
  const load = () => api.ticket(id).then(r => setT(r.data))
  useEffect(() => { load(); api.agents().then(r => setAgents(r.data)) }, [id])
  const flash = (ok, text) => { setMsg({ ok, text }); setTimeout(() => setMsg(null), 2500) }
  const act = async (fn) => { try { await fn(); load(); onChanged() } catch (e) { flash(false, e.message) } }
  const addComment = async () => { if (!comment) return; await act(() => api.comment(id, { author: 'Agent', body: comment, isInternal: internal })); setComment('') }
  if (!t) return <Modal title="…" onClose={onClose}><p className="muted">Đang tải…</p></Modal>
  return (
    <Modal title={`${t.code} — ${t.subject}`} onClose={onClose} wide>
      <Flash msg={msg} />
      <div className="row" style={{ marginBottom: 8 }}><Badge text={STATUS[t.status]} css={STATUS_CSS[t.status]} /><Badge text={PRIORITY[t.priority]} css={PRIORITY_CSS[t.priority]} />{t.overdue && <Badge text="Quá hạn SLA" css="danger" />}</div>
      <dl className="dl"><dt>Khách</dt><dd>{t.customerName}{t.customerPhone ? ` · ${t.customerPhone}` : ''}</dd><dt>Kênh</dt><dd>{t.channelText}</dd>
        <dt>Danh mục</dt><dd>{t.category || '—'}</dd><dt>Hạn SLA</dt><dd>{fmtDateTime(t.dueAt)}</dd></dl>
      {t.description && <div className="card" style={{ background: '#f8fafc' }}>{t.description}</div>}
      <div className="section-t">Xử lý</div>
      <div className="row" style={{ gap: 6, flexWrap: 'wrap' }}>
        <select value={t.status} onChange={e => act(() => api.setStatus(id, Number(e.target.value)))}>{STATUS.map((s, i) => <option key={i} value={i}>{s}</option>)}</select>
        <select value={t.priority} onChange={e => act(() => api.setPriority(id, Number(e.target.value)))}>{PRIORITY.map((p, i) => <option key={i} value={i}>{p}</option>)}</select>
        <select value={t.agentId || ''} onChange={e => act(() => api.assign(id, e.target.value ? Number(e.target.value) : null))}><option value="">— Gán agent —</option>{agents.map(a => <option key={a.id} value={a.id}>{a.name}</option>)}</select>
      </div>
      <div className="section-t">Trao đổi</div>
      <div style={{ maxHeight: 200, overflow: 'auto' }}>{t.comments.map((c, i) => (
        <div key={i} className="card" style={{ background: c.isInternal ? '#fffbeb' : '#f8fafc', marginBottom: 6, padding: 10 }}>
          <b>{c.author}</b> {c.isInternal && <span className="pill">nội bộ</span>} <span className="muted" style={{ fontSize: 12 }}>{fmtDateTime(c.createdAt)}</span><br />{c.body}</div>))}</div>
      <div className="row" style={{ marginTop: 8 }}><Field label="Thêm ghi chú"><input value={comment} onChange={e => setComment(e.target.value)} onKeyDown={e => e.key === 'Enter' && addComment()} /></Field>
        <label style={{ flex: 'none', alignSelf: 'flex-end', display: 'flex', gap: 4, alignItems: 'center', paddingBottom: 8 }}><input type="checkbox" style={{ width: 'auto' }} checked={internal} onChange={e => setInternal(e.target.checked)} />nội bộ</label>
        <div style={{ flex: 'none', alignSelf: 'flex-end' }}><button className="btn sm" onClick={addComment}>Gửi</button></div></div>
    </Modal>
  )
}

function TicketForm({ onClose, onSaved }) {
  const [cats, setCats] = useState([]); const [f, setF] = useState({ subject: '', description: '', customerName: '', customerPhone: '', channel: 0, priority: 1, categoryId: '' }); const [err, setErr] = useState('')
  useEffect(() => { api.categories().then(r => setCats(r.data)) }, [])
  const up = (k, v) => setF({ ...f, [k]: v })
  const save = async () => { try { if (!f.subject || !f.customerName) { setErr('Cần tiêu đề + tên khách'); return } await api.create({ ...f, channel: Number(f.channel), priority: Number(f.priority), categoryId: f.categoryId ? Number(f.categoryId) : null }); onSaved() } catch (e) { setErr(e.message) } }
  return (
    <Modal title="Tạo ticket" onClose={onClose}>
      {err && <Flash msg={{ ok: false, text: err }} />}
      <Field label="Tiêu đề *"><input value={f.subject} onChange={e => up('subject', e.target.value)} /></Field>
      <Field label="Mô tả"><textarea rows={3} value={f.description} onChange={e => up('description', e.target.value)} /></Field>
      <div className="row"><Field label="Khách hàng *"><input value={f.customerName} onChange={e => up('customerName', e.target.value)} /></Field>
        <Field label="SĐT"><input value={f.customerPhone} onChange={e => up('customerPhone', e.target.value)} /></Field></div>
      <div className="row"><Field label="Kênh"><select value={f.channel} onChange={e => up('channel', e.target.value)}>{CHANNELS.map((c, i) => <option key={i} value={i}>{c}</option>)}</select></Field>
        <Field label="Ưu tiên"><select value={f.priority} onChange={e => up('priority', e.target.value)}>{PRIORITY.map((p, i) => <option key={i} value={i}>{p}</option>)}</select></Field>
        <Field label="Danh mục"><select value={f.categoryId} onChange={e => up('categoryId', e.target.value)}><option value="">—</option>{cats.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}</select></Field></div>
      <div style={{ marginTop: 16 }}><button className="btn" onClick={save}>Tạo ticket</button></div>
    </Modal>
  )
}

function Calls() {
  const [rows, setRows] = useState([]); const [stats, setStats] = useState(null); const [show, setShow] = useState(false)
  const load = () => { api.calls().then(r => setRows(r.data)); api.callStats().then(r => setStats(r.data)) }
  useEffect(() => { load() }, [])
  return (
    <>
      <div className="toolbar"><h1 style={{ margin: 0, flex: 1 }}>Cuộc gọi</h1><button className="btn sm" style={{ flex: 'none' }} onClick={() => setShow(true)}>+ Ghi cuộc gọi</button></div>
      {stats && <div className="grid kpis" style={{ marginBottom: 14 }}>
        <div className="kpi"><div className="v">{stats.todayTotal}</div><div className="l">Cuộc gọi hôm nay</div></div>
        <div className="kpi"><div className="v" style={{ color: 'var(--danger)' }}>{stats.missed}</div><div className="l">Nhỡ</div></div>
        <div className="kpi"><div className="v">{Math.floor(stats.avgSeconds / 60)}:{String(stats.avgSeconds % 60).padStart(2, '0')}</div><div className="l">Thời lượng TB</div></div>
      </div>}
      <div className="card" style={{ padding: 0, overflow: 'auto' }}>
        <table><thead><tr><th>Thời gian</th><th>Chiều</th><th>SĐT</th><th>Khách</th><th>Kết quả</th><th className="right">Thời lượng</th></tr></thead>
          <tbody>{rows.map(c => (<tr key={c.id}><td className="muted" style={{ fontSize: 12 }}>{fmtDateTime(c.startedAt)}</td><td>{c.dirText === 'Inbound' ? '📥 Gọi đến' : '📤 Gọi đi'}</td><td>{c.phoneNumber}</td><td>{c.customerName || '—'}</td>
            <td><span className="pill">{c.outcomeText}</span></td><td className="right">{Math.floor(c.durationSeconds / 60)}:{String(c.durationSeconds % 60).padStart(2, '0')}</td></tr>))}
            {rows.length === 0 && <tr><td colSpan={6} className="muted" style={{ padding: 20 }}>Chưa có cuộc gọi.</td></tr>}</tbody></table>
      </div>
      {show && <CallForm onClose={() => setShow(false)} onSaved={() => { setShow(false); load() }} />}
    </>
  )
}

function CallForm({ onClose, onSaved }) {
  const [f, setF] = useState({ direction: 0, outcome: 0, phoneNumber: '', customerName: '', durationSeconds: 0, note: '' }); const [err, setErr] = useState('')
  const up = (k, v) => setF({ ...f, [k]: v })
  const save = async () => { try { await api.logCall({ ...f, direction: Number(f.direction), outcome: Number(f.outcome), durationSeconds: Number(f.durationSeconds) }); onSaved() } catch (e) { setErr(e.message) } }
  return (
    <Modal title="Ghi nhật ký cuộc gọi" onClose={onClose}>
      {err && <Flash msg={{ ok: false, text: err }} />}
      <div className="row"><Field label="Chiều"><select value={f.direction} onChange={e => up('direction', e.target.value)}><option value={0}>Gọi đến</option><option value={1}>Gọi đi</option></select></Field>
        <Field label="Kết quả"><select value={f.outcome} onChange={e => up('outcome', e.target.value)}><option value={0}>Nghe máy</option><option value={1}>Nhỡ</option><option value={2}>Hộp thư</option><option value={3}>Bận</option></select></Field></div>
      <div className="row"><Field label="SĐT"><input value={f.phoneNumber} onChange={e => up('phoneNumber', e.target.value)} /></Field>
        <Field label="Khách"><input value={f.customerName} onChange={e => up('customerName', e.target.value)} /></Field>
        <Field label="Thời lượng (giây)"><input type="number" value={f.durationSeconds} onChange={e => up('durationSeconds', e.target.value)} /></Field></div>
      <Field label="Ghi chú"><input value={f.note} onChange={e => up('note', e.target.value)} /></Field>
      <div style={{ marginTop: 16 }}><button className="btn" onClick={save}>Ghi cuộc gọi</button></div>
    </Modal>
  )
}

function Kb() {
  const [rows, setRows] = useState([]); const [q, setQ] = useState(''); const [open, setOpen] = useState(null)
  const load = () => api.kb(q).then(r => setRows(r.data))
  useEffect(() => { load() }, [])
  return (
    <>
      <div className="toolbar"><h1 style={{ margin: 0, flex: 'none' }}>Cơ sở kiến thức</h1><div className="sp" />
        <input style={{ maxWidth: 240 }} placeholder="Tìm bài viết…" value={q} onChange={e => setQ(e.target.value)} onKeyDown={e => e.key === 'Enter' && load()} />
        <button className="btn ghost sm" style={{ flex: 'none' }} onClick={load}>Tìm</button></div>
      <div className="card" style={{ padding: 0, overflow: 'auto' }}>
        <table><thead><tr><th>Tiêu đề</th><th>Danh mục</th><th className="right">Lượt xem</th></tr></thead>
          <tbody>{rows.map(a => (<tr key={a.id} style={{ cursor: 'pointer' }} onClick={() => api.kbArticle(a.id).then(r => setOpen(r.data))}><td>{a.title}</td><td><span className="pill">{a.category}</span></td><td className="right">{a.views}</td></tr>))}
            {rows.length === 0 && <tr><td colSpan={3} className="muted" style={{ padding: 20 }}>Chưa có bài viết.</td></tr>}</tbody></table>
      </div>
      {open && <Modal title={open.title} onClose={() => setOpen(null)} wide><div className="pill">{open.category}</div><div style={{ whiteSpace: 'pre-wrap', marginTop: 12 }}>{open.body}</div></Modal>}
    </>
  )
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<Dashboard />} />
        <Route path="tickets" element={<Tickets />} />
        <Route path="calls" element={<Calls />} />
        <Route path="kb" element={<Kb />} />
      </Route>
    </Routes>
  )
}
