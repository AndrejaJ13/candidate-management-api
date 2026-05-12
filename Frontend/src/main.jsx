import React, { useEffect, useMemo, useState } from 'react';
import { createRoot } from 'react-dom/client';
import './styles.css';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5184';

async function apiRequest(path, options = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
    ...options,
  });

  if (response.status === 204) {
    return null;
  }

  const contentType = response.headers.get('content-type') ?? '';
  const text = await response.text();
  const data = text && contentType.includes('application/json') ? JSON.parse(text) : text || null;

  if (!response.ok) {
    throw new Error(typeof data === 'string' ? data : text || 'Request failed.');
  }

  return data;
}

function App() {
  const [skills, setSkills] = useState([]);
  const [candidates, setCandidates] = useState([]);
  const [selectedSkillIds, setSelectedSkillIds] = useState([1, 4]);
  const [skillToAddByCandidate, setSkillToAddByCandidate] = useState({});
  const [editingCandidateId, setEditingCandidateId] = useState(null);
  const [searchName, setSearchName] = useState('');
  const [searchSkill, setSearchSkill] = useState('');
  const [newSkillName, setNewSkillName] = useState('');
  const [message, setMessage] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [candidateForm, setCandidateForm] = useState({
    fullName: '',
    dateOfBirth: '',
    contactNumber: '',
    email: '',
  });

  const selectedSkillSet = useMemo(() => new Set(selectedSkillIds), [selectedSkillIds]);

  async function loadInitialData() {
    setIsLoading(true);
    setMessage('');

    try {
      const [skillData, candidateData] = await Promise.all([
        apiRequest('/api/skills'),
        apiRequest('/api/candidates/search'),
      ]);

      setSkills(skillData);
      setCandidates(candidateData);
    } catch (error) {
      setMessage(error.message);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadInitialData();
  }, []);

  async function searchCandidates(event) {
    event.preventDefault();
    setMessage('');

    const params = new URLSearchParams();
    if (searchName.trim()) {
      params.append('name', searchName.trim());
    }

    if (searchSkill.trim()) {
      params.append('skills', searchSkill.trim());
    }

    try {
      const data = await apiRequest(`/api/candidates/search?${params.toString()}`);
      setCandidates(data);
    } catch (error) {
      setMessage(error.message);
    }
  }

  async function createSkill(event) {
    event.preventDefault();
    setMessage('');

    try {
      const createdSkill = await apiRequest('/api/skills', {
        method: 'POST',
        body: JSON.stringify({ name: newSkillName }),
      });

      setSkills((currentSkills) =>
        [...currentSkills, createdSkill].sort((left, right) => left.name.localeCompare(right.name)),
      );
      setNewSkillName('');
      setMessage('Skill created.');
    } catch (error) {
      setMessage(error.message);
    }
  }

  async function saveCandidate(event) {
    event.preventDefault();
    setMessage('');

    try {
      const savedCandidate = await apiRequest(
        editingCandidateId ? `/api/candidates/${editingCandidateId}` : '/api/candidates',
        {
          method: editingCandidateId ? 'PUT' : 'POST',
          body: JSON.stringify({
            ...candidateForm,
            skillIds: selectedSkillIds,
          }),
        },
      );

      setCandidates((currentCandidates) => {
        const withoutSavedCandidate = currentCandidates.filter(
          (candidate) => candidate.id !== savedCandidate.id,
        );

        return [savedCandidate, ...withoutSavedCandidate].sort((left, right) =>
          left.fullName.localeCompare(right.fullName),
        );
      });
      resetCandidateForm();
      setMessage(editingCandidateId ? 'Candidate updated.' : 'Candidate created.');
    } catch (error) {
      setMessage(error.message);
    }
  }

  async function removeCandidate(candidateId) {
    setMessage('');

    try {
      await apiRequest(`/api/candidates/${candidateId}`, { method: 'DELETE' });
      setCandidates((currentCandidates) =>
        currentCandidates.filter((candidate) => candidate.id !== candidateId),
      );
      setMessage('Candidate removed.');
    } catch (error) {
      setMessage(error.message);
    }
  }

  async function addSkillToCandidate(candidateId) {
    const skillId = Number(skillToAddByCandidate[candidateId]);
    if (!skillId) {
      setMessage('Choose a skill first.');
      return;
    }

    setMessage('');

    try {
      const updatedCandidate = await apiRequest(`/api/candidates/${candidateId}/skills/${skillId}`, {
        method: 'POST',
      });

      replaceCandidate(updatedCandidate);
      setSkillToAddByCandidate((currentValues) => ({
        ...currentValues,
        [candidateId]: '',
      }));
      setMessage('Skill added to candidate.');
    } catch (error) {
      setMessage(error.message);
    }
  }

  async function removeSkillFromCandidate(candidateId, skillId) {
    setMessage('');

    try {
      const updatedCandidate = await apiRequest(`/api/candidates/${candidateId}/skills/${skillId}`, {
        method: 'DELETE',
      });

      replaceCandidate(updatedCandidate);
      setMessage('Skill removed from candidate.');
    } catch (error) {
      setMessage(error.message);
    }
  }

  function startEditingCandidate(candidate) {
    setEditingCandidateId(candidate.id);
    setCandidateForm({
      fullName: candidate.fullName,
      dateOfBirth: candidate.dateOfBirth,
      contactNumber: candidate.contactNumber,
      email: candidate.email,
    });
    setSelectedSkillIds(candidate.skills.map((skill) => skill.id));
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  function resetCandidateForm() {
    setEditingCandidateId(null);
    setCandidateForm({
      fullName: '',
      dateOfBirth: '',
      contactNumber: '',
      email: '',
    });
    setSelectedSkillIds([]);
  }

  function replaceCandidate(updatedCandidate) {
    setCandidates((currentCandidates) =>
      currentCandidates
        .map((candidate) => (candidate.id === updatedCandidate.id ? updatedCandidate : candidate))
        .sort((left, right) => left.fullName.localeCompare(right.fullName)),
    );
  }

  function getAvailableSkills(candidate) {
    const candidateSkillIds = new Set(candidate.skills.map((skill) => skill.id));
    return skills.filter((skill) => !candidateSkillIds.has(skill.id));
  }

  function toggleSkill(skillId) {
    setSelectedSkillIds((currentIds) =>
      currentIds.includes(skillId)
        ? currentIds.filter((id) => id !== skillId)
        : [...currentIds, skillId],
    );
  }

  function updateCandidateForm(field, value) {
    setCandidateForm((currentForm) => ({
      ...currentForm,
      [field]: value,
    }));
  }

  return (
    <main className="app-shell">
      <section className="workspace">
        <header className="topbar">
          <div>
            <p className="eyebrow">HR platform</p>
            <h1>Candidate Skills</h1>
          </div>
          <button className="secondary-button" type="button" onClick={loadInitialData}>
            Refresh
          </button>
        </header>

        {message && <p className="status-message">{message}</p>}

        <section className="toolbar" aria-label="Candidate search">
          <form className="search-form" onSubmit={searchCandidates}>
            <label>
              Name
              <input
                value={searchName}
                onChange={(event) => setSearchName(event.target.value)}
                placeholder="Ana"
              />
            </label>
            <label>
              Skill
              <input
                value={searchSkill}
                onChange={(event) => setSearchSkill(event.target.value)}
                placeholder="English"
              />
            </label>
            <button type="submit">Search</button>
          </form>
        </section>

        <section className="content-grid">
          <section className="candidate-list" aria-label="Candidates">
            <div className="section-heading">
              <h2>Candidates</h2>
              <span>{isLoading ? 'Loading' : `${candidates.length} shown`}</span>
            </div>

            <div className="list">
              {candidates.map((candidate) => (
                <article className="candidate-card" key={candidate.id}>
                  <div className="candidate-main">
                    <div>
                      <h3>{candidate.fullName}</h3>
                      <p>{candidate.email}</p>
                      <p>{candidate.contactNumber}</p>
                    </div>
                    <div className="card-actions">
                      <button
                        className="secondary-button"
                        type="button"
                        onClick={() => startEditingCandidate(candidate)}
                      >
                        Edit
                      </button>
                      <button
                        className="danger-button"
                        type="button"
                        onClick={() => removeCandidate(candidate.id)}
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                  <div className="skill-row">
                    {candidate.skills.map((skill) => (
                      <span className="skill-pill removable" key={skill.id}>
                        {skill.name}
                        <button
                          aria-label={`Remove ${skill.name} from ${candidate.fullName}`}
                          type="button"
                          onClick={() => removeSkillFromCandidate(candidate.id, skill.id)}
                        >
                          x
                        </button>
                      </span>
                    ))}
                  </div>
                  <div className="inline-skill-form">
                    <select
                      value={skillToAddByCandidate[candidate.id] ?? ''}
                      onChange={(event) =>
                        setSkillToAddByCandidate((currentValues) => ({
                          ...currentValues,
                          [candidate.id]: event.target.value,
                        }))
                      }
                    >
                      <option value="">Add existing skill</option>
                      {getAvailableSkills(candidate).map((skill) => (
                        <option value={skill.id} key={skill.id}>
                          {skill.name}
                        </option>
                      ))}
                    </select>
                    <button type="button" onClick={() => addSkillToCandidate(candidate.id)}>
                      Add
                    </button>
                  </div>
                </article>
              ))}
            </div>
          </section>

          <aside className="side-panel">
            <form className="form-panel" onSubmit={saveCandidate}>
              <div className="form-heading">
                <h2>{editingCandidateId ? 'Edit Candidate' : 'Add Candidate'}</h2>
                {editingCandidateId && (
                  <button className="secondary-button small-button" type="button" onClick={resetCandidateForm}>
                    Cancel
                  </button>
                )}
              </div>
              <label>
                Full name
                <input
                  required
                  value={candidateForm.fullName}
                  onChange={(event) => updateCandidateForm('fullName', event.target.value)}
                  placeholder="Jelena Petrovic"
                />
              </label>
              <label>
                Date of birth
                <input
                  required
                  type="date"
                  value={candidateForm.dateOfBirth}
                  onChange={(event) => updateCandidateForm('dateOfBirth', event.target.value)}
                />
              </label>
              <label>
                Contact number
                <input
                  required
                  value={candidateForm.contactNumber}
                  onChange={(event) => updateCandidateForm('contactNumber', event.target.value)}
                  placeholder="+38164555666"
                />
              </label>
              <label>
                Email
                <input
                  required
                  type="email"
                  value={candidateForm.email}
                  onChange={(event) => updateCandidateForm('email', event.target.value)}
                  placeholder="jelena.petrovic@example.com"
                />
              </label>

              <div className="checkbox-group" aria-label="Candidate skills">
                {skills.map((skill) => (
                  <label className="checkbox-row" key={skill.id}>
                    <input
                      type="checkbox"
                      checked={selectedSkillSet.has(skill.id)}
                      onChange={() => toggleSkill(skill.id)}
                    />
                    {skill.name}
                  </label>
                ))}
              </div>

              <button type="submit">{editingCandidateId ? 'Save Candidate' : 'Create Candidate'}</button>
            </form>

            <form className="form-panel compact" onSubmit={createSkill}>
              <h2>Add Skill</h2>
              <label>
                Skill name
                <input
                  required
                  value={newSkillName}
                  onChange={(event) => setNewSkillName(event.target.value)}
                  placeholder="React"
                />
              </label>
              <button type="submit">Create Skill</button>
            </form>
          </aside>
        </section>
      </section>
    </main>
  );
}

createRoot(document.getElementById('root')).render(<App />);
